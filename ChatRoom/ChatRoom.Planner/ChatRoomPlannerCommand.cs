using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ChatRoom.SDK;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;

namespace ChatRoom.Planner;

internal class ChatRoomPlannerCommand : ChatRoomAgentCommand
{
    public static string Description => """
        This package contains planners and corresponding orchestrators for chatroom.

        The following planners are available:
        - react-planner: a ReAct agent for multi-agent planning.
        """;
    public override Task<int> ExecuteAsync(CommandContext context, ChatRoomAgentClientCommandSettings settings)
    {
        var config = settings.ConfigFile is not null
            ? JsonSerializer.Deserialize<ChatRoomPlannerConfiguration>(File.ReadAllText(settings.ConfigFile))!
            : new ChatRoomPlannerConfiguration();

        return ExecuteAsync(config);
    }

    internal async Task<int> ExecuteAsync(ChatRoomPlannerConfiguration config)
    {
        _deployed = false;
        _host = Host.CreateDefaultBuilder()
            .UseChatRoomClient(roomName: config.RoomConfig.Room ?? "room", port: config.RoomConfig.Port)
            .Build();

        await _host.StartAsync();
        var chatroomClient = _host.Services.GetRequiredService<ChatPlatformClient>();
        var reactPlanner = ChatroomPlannerAgentFactory.CreateReactPlanner(config.ReActPlannerConfiguration);
        var reactOrchestrator = ChatroomOrchestratorFactory.CreateReactPlanningOrchestrator(config.ReActPlannerConfiguration);
        await chatroomClient.RegisterAutoGenAgentAsync(reactPlanner, config.ReActPlannerConfiguration.Description);
        await chatroomClient.RegisterOrchestratorAsync(nameof(ReactPlanningOrchestrator), reactOrchestrator);

        _deployed = true;

        await _host.WaitForShutdownAsync();

        return 0;
    }
}
