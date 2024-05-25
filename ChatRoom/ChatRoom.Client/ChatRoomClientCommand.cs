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
    [Description("The room name to create.")]
    [CommandOption("-r|--room <ROOM>")]
    public string? Room { get; init; } = null;

    [Description("The port to listen.")]
    [CommandOption("-p|--port <PORT>")]
    public int? Port { get; init; } = null;

    [Description("Your name in the room")]
    [CommandOption("-n|--name <NAME>")]
    public string YourName { get; init; } = "User";

    [Description("Configuration file")]
    [CommandOption("-c|--config <CONFIG>")]
    public string? ConfigFile { get; init; } = null;
}

public class ChatRoomClientCommand : AsyncCommand<ChatRoomClientCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ChatRoomClientCommandSettings command)
    {
        var config = command.ConfigFile is not null
            ? JsonSerializer.Deserialize<ChatRoomClientConfiguration>(File.ReadAllText(command.ConfigFile))!
            : new ChatRoomClientConfiguration();

        config.RoomConfig.Room = command.Room ?? config.RoomConfig.Room;
        config.RoomConfig.Port = command.Port ?? config.RoomConfig.Port;
        config.YourName = command.YourName;

        var host = Host.CreateDefaultBuilder()
            .UseOrleans(siloBuilder =>
            {
                siloBuilder
                    .UseLocalhostClustering(config.RoomConfig.Port)
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
