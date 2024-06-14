using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ChatRoom.Common;
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

    [Description("The workspace to store logs, checkpoints and other files. The default value is the current directory.")]
    [CommandOption("-w|--workspace <WORKSPACE>")]
    public string Workspace { get; init; } = Environment.CurrentDirectory;
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

        var workspace = command.Workspace;
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
        var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();

                var serilogLogger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .WriteTo.File(clientLogPath, outputTemplate: debugLogTemplate)
#if DEBUG
                    .WriteTo.Console(outputTemplate: debugLogTemplate)
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
                serviceCollection.AddSingleton(sp =>
                {
                    var roomObserver = sp.GetRequiredService<ConsoleRoomObserver>();
                    var clusterClient = sp.GetRequiredService<IClusterClient>();
                    var roomObserverRef = clusterClient.CreateObjectReference<IRoomObserver>(roomObserver);
                    return roomObserverRef;
                });
                serviceCollection.AddSingleton<ChatRoomClientController>();
                serviceCollection.AddSingleton<ConsoleChatRoomService>();
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

                 AnsiConsole.MarkupLine($"web ui is available at: [bold blue]{serverConfig.Urls}[/]");
             });
        }

        var host = hostBuilder.Build();

        await host.StartAsync();
        var sp = host.Services;
        var logger = sp.GetRequiredService<ILogger<ChatRoomClientCommand>>();
        logger.LogInformation("Client started.");
        logger.LogInformation($"Workspace: {workspace}");
        logger.LogInformation($"client log is saved to: {Path.Combine(workspace, "logs", clientLogPath)}");
        AnsiConsole.MarkupLine("[bold green]Client started.[/]");
        AnsiConsole.MarkupLine($"[bold green]Workspace:[/] {workspace}");
        AnsiConsole.MarkupLine($"[bold green]client log is saved to:[/] {Path.Combine(workspace, "logs", clientLogPath)}");
        var lifetimeManager = sp.GetRequiredService<IHostApplicationLifetime>();

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
        var consoleChatRoomService = sp.GetRequiredService<ConsoleChatRoomService>();
        await consoleChatRoomService.StartAsync(CancellationToken.None);

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
