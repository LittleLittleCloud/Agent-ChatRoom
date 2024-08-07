﻿using System.ComponentModel;
using System.Text.Json;
using ChatRoom.SDK;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;
using Spectre.Console.Cli;
namespace ChatRoom.OpenAI;
internal class OpenAICommand : ChatRoomAgentCommand
{
    public static string Description { get; } = "Start the chat room with OpenAI agents and configuration.";

    public override async Task<int> ExecuteAsync(CommandContext context, ChatRoomAgentClientCommandSettings setting)
    {
        var config = setting.ConfigFile is not null
            ? JsonSerializer.Deserialize<ChatRoomOpenAIConfiguration>(File.ReadAllText(setting.ConfigFile))!
            : new ChatRoomOpenAIConfiguration();

        return await ExecuteAsync(config);
    }

    internal async Task<int> ExecuteAsync(ChatRoomOpenAIConfiguration config)
    {
        // verify if the name of agents is duplicated
        var agentNames = config.Agents.Select(agent => agent.Name);
        if (agentNames.Distinct().Count() != agentNames.Count())
        {
            AnsiConsole.MarkupLine("[red]The name of agents can't be duplicated.[/]");
            return 1;
        }

        _host = Host.CreateDefaultBuilder()
            .UseChatRoomClient(roomName: config.RoomConfig.Room ?? "room", port: config.RoomConfig.Port)
            .Build();
        await _host.StartAsync();
        var chatRoomClient = _host.Services.GetRequiredService<ChatPlatformClient>();

        foreach (var agentConfig in config.Agents)
        {
            var agentFactory = new OpenAIAgentFactory(agentConfig);

            await chatRoomClient.RegisterAutoGenAgentAsync(agentFactory.CreateAgent(), agentConfig.Description);
        }

        _deployed = true;
        await _host.WaitForShutdownAsync();

        return 0;
    }
}
