using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using AutoGen.Core;
using ChatRoom.Common;
using Spectre.Console;

namespace ChatRoom.Client;

internal class ConsoleChannelObserver : IChannelObserver
{
    private readonly string name;
    private readonly string channelName;

    public ConsoleChannelObserver(string name, string channelName)
    {
        this.name = name;
        this.channelName = channelName;
    }

    public Task<ChatMsg?> GenerateReplyAsync(AgentInfo agent, ChatMsg[] msg) => Task.FromResult<ChatMsg?>(null);

    public Task Join(AgentInfo agent)
    {
        if (agent.Name != name)
        {
            AnsiConsole.MarkupLine($"[green]{agent.Name}[/] joined the [yellow]{channelName}[/] channel.");
        }

        return Task.CompletedTask;
    }
    public Task Leave(AgentInfo agent)
    {
        if (agent.Name != name)
        {
            AnsiConsole.MarkupLine($"[green]{agent.Name}[/] left the [yellow]{channelName}[/] channel.");
        }

        return Task.CompletedTask;
    }

    public Task Notification(ChatMsg msg)
    {
        AnsiConsole.MarkupLine($"[grey]{msg.From}[/]: {msg.Text}");

        return Task.CompletedTask;
    }

    public Task NewMessage(ChatMsg msg)
    {
        var text = msg.Text;
        text = text.Replace("[", "[[");
        text = text.Replace("]", "]]");
        AnsiConsole.MarkupLine(
            "[[[dim]{0}[/]]][[{1}]] [bold yellow]{2}:[/] {3}",
            msg.Created.LocalDateTime, channelName, msg.From!, text);

        return Task.CompletedTask;
    }
}
