using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public static string Description => """
        Powershell agents for chat room.

        The following agents are available:
        - ps-gpt: an agent that generate powershell script.
        - ps-runner: an agent that run powershell script.

        To use the agents, you need to provide a configuration file.
        A configuration file is a json file with the following schema:
        - https://raw.githubusercontent.com/LittleLittleCloud/Agent-ChatRoom/main/schema/chatroom_powershell_configuration_schema.json
        """;
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
