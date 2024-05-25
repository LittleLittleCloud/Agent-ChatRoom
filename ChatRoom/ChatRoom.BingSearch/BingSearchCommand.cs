using System.Text.Json;
using ChatRoom.BingSearch;
using ChatRoom.SDK;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;

public class BingSearchCommand : ChatRoomAgentCommand
{
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

