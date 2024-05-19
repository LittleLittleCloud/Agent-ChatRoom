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

    public Task Join(AgentInfo agent)
    {
        AnsiConsole.MarkupLine($"[green]{agent.Name}[/] joins the chat room.");

        return Task.CompletedTask;
    }
    public Task Leave(AgentInfo agent)
    {
        AnsiConsole.MarkupLine($"[green]{agent.Name}[/] leaves the chat room.");

        return Task.CompletedTask;
    }

    public Task Notification(ChatMsg msg)
    {
        AnsiConsole.MarkupLine($"[grey]{msg.From}[/]: {msg.Text}");

        return Task.CompletedTask;
    }

    public Task RemoveMemberFromChannel(ChannelInfo channel, AgentInfo agent)
    {
        return Task.CompletedTask;
    }
}
