using System.Text.Json;
using ChatRoom.BingSearch;
using ChatRoom.SDK;
using Json.Schema.Generation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;

internal class BingSearchCommand : AsyncCommand<ChatRoomAgentClientCommandSettings>
{
    private IHost? _host = null;
    private bool _deployed = false;

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

    internal async Task<int> ExecuteAsync(BingSearchConfiguration config)
    {
        _deployed = false;
        _host = Host.CreateDefaultBuilder()
            //.AddAgentAsync(async (_) => AgentFactory.CreateBingSearchAgent(config), config.Description)
            .UseChatRoomClient(roomName: config.RoomConfig.Room ?? "room", port: config.RoomConfig.Port)
            .Build();

        await _host.StartAsync();
        var chatroomClient = _host.Services.GetRequiredService<ChatPlatformClient>();

        await chatroomClient.RegisterAutoGenAgentAsync(AgentFactory.CreateBingSearchAgent(config), config.Description);

        _deployed = true;

        await _host.WaitForShutdownAsync();

        return 0;
    }
}

