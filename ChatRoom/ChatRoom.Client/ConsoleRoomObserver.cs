using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatRoom.Common;
using Spectre.Console;

namespace ChatRoom.Client;

internal class ConsoleRoomObserver : IRoomObserver
{
    public Task AddMemberToChannel(ChannelInfo channel, AgentInfo agent)
    {
        return Task.CompletedTask;
    }

    public Task<ChatMsg?> GenerateReplyAsync(AgentInfo agent, ChatMsg[] msg, ChannelInfo _)
    {
        return Task.FromResult<ChatMsg?>(null);
    }

    public Task JoinRoom(AgentInfo agent, string room)
    {
        AnsiConsole.MarkupLine($"[green]{agent.Name}[/] joins the [yellow]{room}[/] room.");
        return Task.CompletedTask;
    }

    public Task JoinChannel(AgentInfo agent, ChannelInfo channelInfo)
    {
        AnsiConsole.MarkupLine($"[green]{agent.Name}[/] joins the [yellow]{channelInfo.Name}[/] channel.");
        return Task.CompletedTask;
    }

    public Task LeaveRoom(AgentInfo agent, string room)
    {
        AnsiConsole.MarkupLine($"[green]{agent.Name}[/] leaves the [yellow]{room}[/] room.");

        return Task.CompletedTask;
    }

    public Task LeaveChannel(AgentInfo agent, ChannelInfo channelInfo)
    {
        AnsiConsole.MarkupLine($"[green]{agent.Name}[/] leaves the [yellow]{channelInfo.Name}[/] channel.");

        return Task.CompletedTask;
    }

    public Task NewMessage(ChatMsg msg)
    {
        var text = msg.Text;
        text = text.Replace("[", "[[");
        text = text.Replace("]", "]]");
        AnsiConsole.MarkupLine(
            "[[[dim]{0}[/]]] [bold yellow]{1}:[/] {2}",
            msg.Created.LocalDateTime, msg.From!, text);

        return Task.CompletedTask;
    }

    public Task Notification(ChatMsg msg)
    {
        AnsiConsole.MarkupLine($"[grey]{msg.From}[/]: {msg.Text}");

        return Task.CompletedTask;
    }

    public Task<bool> Ping() => Task.FromResult(true);
}
