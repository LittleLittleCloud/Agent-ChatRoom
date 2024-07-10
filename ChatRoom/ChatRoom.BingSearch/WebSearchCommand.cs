using System.Text.Json;
using ChatRoom.WebSearch;
using ChatRoom.SDK;
using Json.Schema.Generation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;

internal class WebSearchCommand : ChatRoomAgentCommand
{
    public static string Description { get; } = """
        Web search agents for chat room.
        This package provides the following web search agents:
        - Bing search agent
        - Google search agent
        
        To use the agent, you need to provide a configuration file.
        A configuration file is a json file with the following schema:
        - https://raw.githubusercontent.com/LittleLittleCloud/Agent-ChatRoom/main/schema/chatroom_bing_search_configuration_schema.json
        """;

    public override async Task<int> ExecuteAsync(CommandContext context, ChatRoomAgentClientCommandSettings settings)
    {
        var config = settings.ConfigFile is not null
            ? JsonSerializer.Deserialize<ChatRoomWebSearchConfiguration>(File.ReadAllText(settings.ConfigFile))!
            : new ChatRoomWebSearchConfiguration();

        return await ExecuteAsync(config);
    }


    internal async Task<int> ExecuteAsync(ChatRoomWebSearchConfiguration config)
    {
        _deployed = false;
        _host = Host.CreateDefaultBuilder()
            .UseChatRoomClient(roomName: config.RoomConfig.Room ?? "room", port: config.RoomConfig.Port)
            .Build();

        await _host.StartAsync();
        var chatroomClient = _host.Services.GetRequiredService<ChatPlatformClient>();

        if (config.BingSearchConfiguration is not null)
        {
            await chatroomClient.RegisterAutoGenAgentAsync(WebSearchAgentFactory.CreateBingSearchAgent(config.BingSearchConfiguration), config.BingSearchConfiguration.Description);
        }

        if (config.GoogleSearchConfiguration is not null)
        {
            await chatroomClient.RegisterAutoGenAgentAsync(WebSearchAgentFactory.CreateGoogleSearchAgent(config.GoogleSearchConfiguration), config.GoogleSearchConfiguration.Description);
        }

        _deployed = true;

        await _host.WaitForShutdownAsync();

        return 0;
    }
}

