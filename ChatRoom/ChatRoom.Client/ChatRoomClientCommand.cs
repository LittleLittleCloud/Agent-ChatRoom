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

        var host = Host.CreateDefaultBuilder()
            .UseOrleans(siloBuilder =>
            {
                siloBuilder
                    .UseLocalhostClustering(gatewayPort: config.RoomConfig.Port)
                    .AddMemoryGrainStorage("PubSubStore")
                    .ConfigureLogging(logBuilder =>
                    {
                        logBuilder
                            .ClearProviders();
                    });
            })
            .ConfigureServices(serviceCollection =>
            {
                serviceCollection.AddSingleton(config);
                serviceCollection.AddSingleton(config.RoomConfig);
                serviceCollection.AddSingleton(config.ChannelConfig);
                serviceCollection.AddHostedService<ConsoleChatRoomService>();
            })
            .Build();

        await host.RunAsync();

        return 0;
    }
}
