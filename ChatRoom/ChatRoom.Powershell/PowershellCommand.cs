using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ChatRoom.SDK;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;

namespace ChatRoom.Powershell;

public class PowershellCommand : ChatRoomAgentCommand
{
    public override async Task<int> ExecuteAsync(CommandContext context, ChatRoomAgentClientCommandSettings settings)
    {
        var config = settings.ConfigFile is not null
            ? JsonSerializer.Deserialize<PowershellConfiguration>(File.ReadAllText(settings.ConfigFile))!
            : new PowershellConfiguration();

        var host = Host.CreateDefaultBuilder()
            .AddAgentAsync(async (_) => AgentFactory.CreatePwshDeveloperAgent(config.GPT), config.GPT.Description)
            .AddAgentAsync(async (_) => new PowershellRunnerAgent(config.Runner.Name, config.Runner.LastNMessage), config.Runner.Description)
            .UseChatRoom(roomName: settings.Room ?? "room", port: settings.Port ?? 30000)
            .Build();

        await host.StartAsync();
        await host.WaitForAgentsJoinRoomAsync();
        await host.WaitForShutdownAsync();

        return 0;
    }
}
