using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;

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

        var dateTimeNow = DateTime.Now;
        var clientLogPath = Path.Combine(workspace, "logs", $"clients-{dateTimeNow:yyyy-MM-dd_HH-mm-ss}.log");
        var debugLogTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] ({SourceContext}) {Message:lj}{NewLine}{Exception}";
        var host = Host.CreateDefaultBuilder()
            .ConfigureLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();

                // log to file: workspace/logs/clients-{YYYY-MM-DD HH-mm-ss}.log
                var logPath = Path.Combine(workspace, "logs", clientLogPath);
                var serilogLogger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .WriteTo.File(logPath, outputTemplate: debugLogTemplate)
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
                serviceCollection.AddHostedService<AgentExtensionBootstrapService>();
                serviceCollection.AddSingleton<ConsoleChatRoomService>();
            })
            .Build();
        await host.StartAsync();
        var sp = host.Services;
        var logger = sp.GetRequiredService<ILogger<ChatRoomClientCommand>>();
        logger.LogInformation("Client started.");
        logger.LogInformation($"Workspace: {workspace}");
        logger.LogInformation($"client log is saved to: {Path.Combine(workspace, "logs", clientLogPath)}");
        AnsiConsole.MarkupLine("[bold green]Client started.[/]");
        var consoleChatRoomService = sp.GetRequiredService<ConsoleChatRoomService>();
        await consoleChatRoomService.StartAsync(CancellationToken.None);

        await host.StopAsync();
        await host.WaitForShutdownAsync();
        return 0;
    }
}
