using System.Text.Json;
using ChatRoom.BingSearch;
using ChatRoom.SDK;
using Json.Schema.Generation;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;

public class BingSearchCommand : AsyncCommand<ChatRoomAgentClientCommandSettings>
{
    public static string Description { get; } = """
        Bing search agent for chat room.
        The agent will search the query from Bing search engine and return the result.
        
        To use the agent, you need to provide a configuration file.
        A configuration file is a json file with the following schema:
        - https://raw.githubusercontent.com/LittleLittleCloud/Agent-ChatRoom/main/schema/chatroom_bing_search_configuration_schema.json
        """;

    public override async Task<int> ExecuteAsync(CommandContext context, ChatRoomAgentClientCommandSettings settings)
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

