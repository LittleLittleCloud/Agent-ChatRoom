using System.Text.Json;
using ChatRoom.WebSearch;
using ChatRoom.SDK;
using Json.Schema.Generation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;

internal class WebSearchCommand : AsyncCommand<ChatRoomAgentClientCommandSettings>
{
    private IHost? _host = null;
    private bool _deployed = false;

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
            ? JsonSerializer.Deserialize<WebSearchConfiguration>(File.ReadAllText(settings.ConfigFile))!
            : new WebSearchConfiguration();

        return await ExecuteAsync(config);
    }


    internal IServiceProvider? ServiceProvider => _host?.Services;

    internal async Task StopAsync(int maxWaitingTimeInSeconds = 10)
    {
        if (_host is not null)
        {
            var timeout = Task.Delay(TimeSpan.FromSeconds(maxWaitingTimeInSeconds));
            var stopHostTask = _host.StopAsync();

            await Task.WhenAny(timeout, stopHostTask);

            if (timeout.IsCompleted)
            {
                throw new TimeoutException("Stop host timeout");
            }
        }
    }

    internal async Task DeployAsync(int maxWaitingTimeInSeconds = 20)
    {
        var timeOut = Task.Delay(TimeSpan.FromSeconds(maxWaitingTimeInSeconds));
        while (true)
        {
            if (_host is null)
            {
                await Task.Delay(1000);
                continue;
            }

            if (_deployed)
            {
                break;
            }

            if (timeOut.IsCompleted)
            {
                throw new TimeoutException("Deploy timeout");
            }

            await Task.Delay(1000);
        }
    }

    internal async Task<int> ExecuteAsync(WebSearchConfiguration config)
    {
        _deployed = false;
        _host = Host.CreateDefaultBuilder()
            .UseChatRoomClient(roomName: config.RoomConfig.Room ?? "room", port: config.RoomConfig.Port)
            .Build();

        await _host.StartAsync();
        var chatroomClient = _host.Services.GetRequiredService<ChatPlatformClient>();

        if (config.BingSearchConfiguration is not null)
        {
            await chatroomClient.RegisterAutoGenAgentAsync(AgentFactory.CreateBingSearchAgent(config.BingSearchConfiguration), config.BingSearchConfiguration.Description);
        }

        if (config.GoogleSearchConfiguration is not null)
        {
            await chatroomClient.RegisterAutoGenAgentAsync(AgentFactory.CreateGoogleSearchAgent(config.GoogleSearchConfiguration), config.GoogleSearchConfiguration.Description);
        }

        _deployed = true;

        await _host.WaitForShutdownAsync();

        return 0;
    }
}

