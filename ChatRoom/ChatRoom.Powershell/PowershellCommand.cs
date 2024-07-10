using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ChatRoom.SDK;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;

namespace ChatRoom.Powershell;

internal class PowershellCommand : ChatRoomAgentCommand
{
    public static string Description => """
        Powershell agents for chat room.

        The following agents are available:
        - ps-gpt: an agent that generate powershell script.
        - ps-runner: an agent that run powershell script.
        """;
    public override async Task<int> ExecuteAsync(CommandContext context, ChatRoomAgentClientCommandSettings settings)
    {
        var config = settings.ConfigFile is not null
            ? JsonSerializer.Deserialize<ChatRoomPowershellConfiguration>(File.ReadAllText(settings.ConfigFile))!
            : new ChatRoomPowershellConfiguration();

        return await ExecuteAsync(config);
    }

    internal async Task<int> ExecuteAsync(ChatRoomPowershellConfiguration config)
    {
        _deployed = false;
        _host = Host.CreateDefaultBuilder()
            .UseChatRoomClient(roomName: config.RoomConfig.Room ?? "room", port: config.RoomConfig.Port)
            .Build();

        await _host.StartAsync();
        var chatroomClient = _host.Services.GetRequiredService<ChatPlatformClient>();
        var psGPTAgent = PowershellAgentFactory.CreatePwshDeveloperAgent(config.GPT);
        var psRunnerAgent = new PowershellRunnerAgent(config.Runner.Name, config.Runner.LastNMessage);
        await chatroomClient.RegisterAutoGenAgentAsync(psGPTAgent, config.GPT.Description);
        await chatroomClient.RegisterAutoGenAgentAsync(psRunnerAgent, config.Runner.Description);

        _deployed = true;
        await _host.WaitForShutdownAsync();

        return 0;
    }
}
