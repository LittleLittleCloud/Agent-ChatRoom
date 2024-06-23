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
    private IHost? _host = null;
    private bool _deployed = false;

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

        return await ExecuteAsync(config);
    }

    internal IServiceProvider? ServiceProvider => _host?.Services;

    internal async Task StopAsync(int maxWaitingTimeInSeconds =10)
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

    internal async Task<int> ExecuteAsync(PowershellConfiguration config)
    {
        _deployed = false;
        _host = Host.CreateDefaultBuilder()
            .UseChatRoom(roomName: config.RoomConfig.Room ?? "room", port: config.RoomConfig.Port)
            .Build();

        await _host.StartAsync();
        var chatroomClient = _host.Services.GetRequiredService<ChatPlatformClient>();
        var psGPTAgent = AgentFactory.CreatePwshDeveloperAgent(config.GPT);
        var psRunnerAgent = new PowershellRunnerAgent(config.Runner.Name, config.Runner.LastNMessage);
        await chatroomClient.RegisterAutoGenAgentAsync(psGPTAgent, config.GPT.Description);
        await chatroomClient.RegisterAutoGenAgentAsync(psRunnerAgent, config.Runner.Description);

        _deployed = true;
        await _host.WaitForShutdownAsync();

        return 0;
    }
}
