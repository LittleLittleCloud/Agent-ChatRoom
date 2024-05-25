using System.Text.Json;
using ChatRoom.SDK;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;
namespace ChatRoom.OpenAI;

internal class OpenAICommand : ChatRoomAgentCommand
{
    public override async Task<int> ExecuteAsync(CommandContext context, ChatRoomAgentClientCommandSettings setting)
    {
        var config = setting.ConfigFile is not null
            ? JsonSerializer.Deserialize<OpenAIAgentConfiguration>(File.ReadAllText(setting.ConfigFile))!
            : new OpenAIAgentConfiguration();

        var agentFactory = new OpenAIAgentFactory(config);

        var host = Host.CreateDefaultBuilder()
            .AddAgentAsync(async (_) => agentFactory.CreateAgent())
            .UseChatRoom(roomName: setting.Room ?? "room", port: setting.Port ?? 30000)
            .Build();

        await host.StartAsync();
        await host.WaitForAgentsJoinRoomAsync();
        await host.WaitForShutdownAsync();

        return 0;
    }
}
