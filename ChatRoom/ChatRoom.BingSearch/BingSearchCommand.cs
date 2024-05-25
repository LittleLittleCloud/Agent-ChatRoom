using System.Text.Json;
using ChatRoom.BingSearch;
using ChatRoom.SDK;
using Json.Schema.Generation;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;

public class BingSearchCommandSettings : ChatRoomAgentClientCommandSettings
{
    [CommandOption("-c|--config <CONFIG>")]
    [Description("Configuration file, schema: https://raw.githubusercontent.com/LittleLittleCloud/Agent-ChatRoom/main/schema/chatroom_bing_search_configuration_schema.json")]
    public override string? ConfigFile { get; init; }
}

public class BingSearchCommand : AsyncCommand<BingSearchCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, BingSearchCommandSettings settings)
    {
        var config = settings.ConfigFile is not null
            ? JsonSerializer.Deserialize<BingSearchConfiguration>(File.ReadAllText(settings.ConfigFile))!
            : new BingSearchConfiguration();

        var host = Host.CreateDefaultBuilder()
            .AddAgentAsync(async (_) => AgentFactory.CreateBingSearchAgent(config), config.Description)
            .UseChatRoom(roomName: settings.Room ?? "room", port: settings.Port ?? 30000)
            .Build();

        await host.StartAsync();
        await host.WaitForAgentsJoinRoomAsync();
        await host.WaitForShutdownAsync();

        return 0;
    }
}

