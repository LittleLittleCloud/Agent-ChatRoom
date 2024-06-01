using AutoGen.Core;
using ChatRoom.SDK;

namespace ChatRoom.Common;

internal class AutoGenAgentObserver : IRoomObserver
{
    private readonly IAgent _agent;
    private readonly IClusterClient _client;

    public AutoGenAgentObserver(IClusterClient client, IAgent agent)
    {
        _client = client;
        _agent = agent;
    }

    public async Task JoinChannel(AgentInfo agent, ChannelInfo channelInfo)
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

    public Task LeaveChannel(AgentInfo agent, ChannelInfo channelInfo)
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

    public async Task<ChatMsg?> GenerateReplyAsync(
        AgentInfo agent,
        ChatMsg[] messages,
        ChannelInfo channelInfo)
    { 
        if (agent.Name != _agent.Name)
        {
            return null;
        }
        // convert ChatMsg to TextMessage
        var textMessages = messages.Select(msg => new TextMessage(Role.Assistant, msg.Text, from: msg.From)).ToArray();
        var channel = _client.GetGrain<IChannelGrain>(channelInfo.Name);
        var eventHandler = new EventHandler<ChatMsg>(async (sender, msg) => await channel.SendNotification(msg));
        if (_agent is INotifyAgent notifyAgent)
        {
            notifyAgent.Notify += eventHandler;
        }

        var reply = await _agent!.GenerateReplyAsync(textMessages);

        if (_agent is INotifyAgent notifyAgent2)
        {
            notifyAgent2.Notify -= eventHandler;
        }

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

    public Task JoinRoom(AgentInfo agent, string room)
    {
        Console.WriteLine($"Agent {agent.Name} joined room {room}");

        return Task.CompletedTask;
    }

    public Task LeaveRoom(AgentInfo agent, string room)
    {
        Console.WriteLine($"Agent {agent.Name} left room {room}");

        return Task.CompletedTask;
    }
}
