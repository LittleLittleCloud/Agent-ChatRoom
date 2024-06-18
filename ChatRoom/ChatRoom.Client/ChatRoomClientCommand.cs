using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ChatRoom.OpenAI;
using ChatRoom.SDK;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;
using Swashbuckle.AspNetCore;

namespace ChatRoom.Client;

public class ChatRoomClientCommandSettings : CommandSettings
{
    [Description("Configuration file, schema: https://raw.githubusercontent.com/LittleLittleCloud/Agent-ChatRoom/main/schema/client_configuration_schema.json")]
    [CommandOption("-c|--config <CONFIG>")]
    public string? ConfigFile { get; init; } = null;
}

public class ChatRoomClientCommand : AsyncCommand<ChatRoomClientCommandSettings>
{
    public static string Description { get; } = """
        A Chatroom cli client.
        
        The client will start a chat room service and attach a console client to it.
        
        To use the client, you need to provide a configuration file.
        A configuration file is a json file with the following schema:
        - https://raw.githubusercontent.com/LittleLittleCloud/Agent-ChatRoom/main/schema/client_configuration_schema.json
        """;
    public override async Task<int> ExecuteAsync(CommandContext context, ChatRoomClientCommandSettings command)
    {
        var config = command.ConfigFile is not null
            ? JsonSerializer.Deserialize<ChatRoomClientConfiguration>(File.ReadAllText(command.ConfigFile))!
            : new ChatRoomClientConfiguration();

        var workspace = config.Workspace;
        if (!Directory.Exists(workspace))
        {
            Directory.CreateDirectory(workspace);
        }

        var clientContext = new ClientContext()
        {
            UserName = config.YourName,
            CurrentRoom = config.RoomConfig.Room,
        };

        var dateTimeNow = DateTime.Now;
        var clientLogPath = Path.Combine(workspace, "logs", $"clients-{dateTimeNow:yyyy-MM-dd_HH-mm-ss}.log");
        var debugLogTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] ({SourceContext}) {Message:lj}{NewLine}{Exception}";
        var infoLogTemplate = "{Message:lj}{NewLine}{Exception}";
        var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();

                var serilogLogger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .WriteTo.File(clientLogPath, outputTemplate: debugLogTemplate)
#if DEBUG
                    .WriteTo.Console(outputTemplate: debugLogTemplate)
#else
                    .WriteTo.Conditional((le) => le.Level >= Serilog.Events.LogEventLevel.Information, lc => lc.Console(outputTemplate: infoLogTemplate))
#endif
                    .CreateLogger();

                loggingBuilder.AddSerilog(serilogLogger);
            })
            .UseOrleans(siloBuilder =>
            {
                siloBuilder
                    .UseLocalhostClustering(gatewayPort: config.RoomConfig.Port)
                    .AddMemoryGrainStorage("PubSubStore");
            })
            .ConfigureServices(serviceCollection =>
            {
                serviceCollection.AddSingleton(config);
                serviceCollection.AddSingleton(config.RoomConfig);
                serviceCollection.AddSingleton(config.ChannelConfig);
                serviceCollection.AddSingleton(command);
                serviceCollection.AddHostedService<AgentExtensionBootstrapService>();

                serviceCollection.AddSingleton(clientContext);
                serviceCollection.AddSingleton<ConsoleRoomObserver>();
                serviceCollection.AddSingleton<RoundRobinOrchestrator>();
                serviceCollection.AddSingleton(sp =>
                {
                    var settings = sp.GetRequiredService<ChatRoomClientConfiguration>();

                    return new HumanToAgent(settings.ChannelConfig.OpenAIConfiguration);
                });
                serviceCollection.AddSingleton(sp =>
                {
                    var settings = sp.GetRequiredService<ChatRoomClientConfiguration>();

                    return new DynamicGroupChat(settings.ChannelConfig.OpenAIConfiguration);
                });
                serviceCollection.AddSingleton(sp =>
                {
                    var clusterClient = sp.GetRequiredService<IClusterClient>();
                    var settings = sp.GetRequiredService<ChatRoomClientConfiguration>();
                    var chatPlatformClient = new ChatPlatformClient(clusterClient, settings.RoomConfig.Room);
                    return chatPlatformClient;
                });
                serviceCollection.AddSingleton(sp =>
                {
                    var roomObserver = sp.GetRequiredService<ConsoleRoomObserver>();
                    var clusterClient = sp.GetRequiredService<IClusterClient>();
                    var roomObserverRef = clusterClient.CreateObjectReference<IRoomObserver>(roomObserver);
                    return roomObserverRef;
                });
                serviceCollection.AddSingleton<ChatRoomClientController>();
                serviceCollection.AddSingleton<ChatRoomConsoleApp>();
            });

        if (config.ServerConfig is ServerConfiguration serverConfig)
        {
            hostBuilder.ConfigureWebHostDefaults(builder =>
             {
                 var assemblyLocation = Assembly.GetExecutingAssembly().Location;
                 var assemblyDirectory = Path.GetDirectoryName(assemblyLocation) ?? Environment.CurrentDirectory;
                 var webRoot = Path.Combine(assemblyDirectory, "wwwroot");
                 Console.WriteLine($"web root: {webRoot}");
                 builder
                 .UseWebRoot(webRoot)
                 .UseContentRoot(workspace)
                 .UseEnvironment(serverConfig.Environment)
                 .UseUrls(serverConfig.Urls)
                 .UseStartup<Startup>();
             });
        }

        var host = hostBuilder.Build();

        var sp = host.Services;
        var lifetimeManager = sp.GetRequiredService<IHostApplicationLifetime>();
        await host.StartAsync();
        await AnsiConsole.Status()
            .StartAsync("initializing...", async ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);

                do
                {
                    await Task.Delay(1000);
                }
                while (!lifetimeManager.ApplicationStarted.IsCancellationRequested);
            });


        var logger = sp.GetRequiredService<ILogger<ChatRoomClientCommand>>();

        // configure chatroom client
        var chatPlatformClient = sp.GetRequiredService<ChatPlatformClient>();
        var observerRef = sp.GetRequiredService<IRoomObserver>();
        var roudRobinOrchestrator = sp.GetRequiredService<RoundRobinOrchestrator>();
        var humanToAgent = sp.GetRequiredService<HumanToAgent>();
        var dynamicGroupChat = sp.GetRequiredService<DynamicGroupChat>();

        await chatPlatformClient.RegisterOrchestratorAsync("RoundRobin", roudRobinOrchestrator);
        await chatPlatformClient.RegisterOrchestratorAsync("HumanToAgent", humanToAgent);
        await chatPlatformClient.RegisterOrchestratorAsync("DynamicGroupChat", dynamicGroupChat);
        await chatPlatformClient.RegisterAgentAsync(config.YourName, clientContext.Description, true, observerRef);
        logger.LogInformation("Client started.");
        logger.LogInformation($"Workspace: {workspace}");
        logger.LogInformation($"client log is saved to: {Path.Combine(workspace, "logs", clientLogPath)}");
        if (config.ServerConfig is ServerConfiguration)
        {
            logger.LogInformation($"web ui is available at: {config.ServerConfig.Urls}");
        }

        if (config.EnableConsoleApp)
        {
            var consoleApp = sp.GetRequiredService<ChatRoomConsoleApp>();
            await consoleApp.StartAsync(CancellationToken.None);
        }
        else
        {
            await host.WaitForShutdownAsync();
        }

        await AnsiConsole.Status()
            .StartAsync("shutting down...", async ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                await host.StopAsync();
                await host.WaitForShutdownAsync();
            });

        return 0;
    }
}
