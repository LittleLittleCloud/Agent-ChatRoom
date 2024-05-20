using AutoGen.Core;

namespace ChatRoom.Common;

internal class AgentObserver : IRoomObserver
{
    private readonly IAgent _agent;

    public AgentObserver(IAgent agent)
    {
        _agent = agent;
    }

    public async Task Join(AgentInfo agent, ChannelInfo channelInfo)
    {
        if (agent.Name != _agent.Name)
        {
            Console.WriteLine($"Agent {agent.Name} joined channel {channelInfo.Name}");
        }
        else
        {
            Console.WriteLine($"You were added to channel {channelInfo.Name}");
        }
    }

    public Task Leave(AgentInfo agent, ChannelInfo channelInfo)
    {
        if (agent.Name != _agent.Name)
        {
            Console.WriteLine($"Agent {agent.Name} left channel {channelInfo.Name}");
        }
        else
        {
            Console.WriteLine($"You were removed from channel {channelInfo.Name}");
        }

        return Task.CompletedTask;
    }

    public async Task<ChatMsg?> GenerateReplyAsync(AgentInfo agent, ChatMsg[] messages)
    { 
        if (agent.Name != _agent.Name)
        {
            return null;
        }
        // convert ChatMsg to TextMessage
        var textMessages = messages.Select(msg => new TextMessage(Role.Assistant, msg.Text, from: msg.From)).ToArray();
        var reply = await _agent!.GenerateReplyAsync(textMessages);

        if (reply.GetContent() is string content)
        {
            return new ChatMsg(_agent.Name, content);
        }

        return null;
    }

    public Task Notification(ChatMsg msg)
    {
        Console.WriteLine($"Notification: {msg.Text}");
        return Task.CompletedTask;
    }

    public Task NewMessage(ChatMsg msg)
    {
        Console.WriteLine($"Message from {msg.From}: {msg.Text}");
        return Task.CompletedTask;
    }

    public Task<bool> Ping() => Task.FromResult(true);

    public Task Join(AgentInfo agent, string room)
    {
        Console.WriteLine($"Agent {agent.Name} joined room {room}");

        return Task.CompletedTask;
    }

    public Task Leave(AgentInfo agent, string room)
    {
        Console.WriteLine($"Agent {agent.Name} left room {room}");

        return Task.CompletedTask;
    }
}
