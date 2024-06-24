using AutoGen.Core;
using ChatRoom.SDK;

namespace ChatRoom.SDK;

internal class AutoGenAgentObserver : IChatRoomAgentObserver
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
        AgentInfo _,
        ChatMsg[] messages,
        ChannelInfo channelInfo)
    { 
        // convert ChatMsg to TextMessage
        var autogenMessages = messages.Select(msg => msg.ToAutoGenMessage()).ToArray();
        var channel = _client.GetGrain<IChannelGrain>(channelInfo.Name);
        var eventHandler = new EventHandler<ChatMsg>(async (sender, msg) => await channel.SendNotification(msg));
        if (_agent is INotifyAgent notifyAgent)
        {
            notifyAgent.Notify += eventHandler;
        }

        var reply = await _agent!.GenerateReplyAsync(autogenMessages);

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
        Console.WriteLine($"Notification: {msg.GetContent()}");
        return Task.CompletedTask;
    }

    public Task NewMessage(ChatMsg msg)
    {
        Console.WriteLine($"Message from {msg.From}: {msg.GetContent()}");
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
