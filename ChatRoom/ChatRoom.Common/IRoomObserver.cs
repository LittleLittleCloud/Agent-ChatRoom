using AutoGen.Core;
using Orleans.Concurrency;

namespace ChatRoom.Common;

public interface INotificationObserver : IGrainObserver
{
    Task Notification(ChatMsg msg);
}

public interface IRoomObserver : INotificationObserver
{
    [OneWay]
    Task Join(AgentInfo agent);

    [OneWay]
    Task Leave(AgentInfo agent);

    //[OneWay]
    Task AddMemberToChannel(ChannelInfo channel, AgentInfo agent);

    //[OneWay]
    Task RemoveMemberFromChannel(ChannelInfo channel, AgentInfo agent);
}

public interface IOrchestratorObserver : INotificationObserver
{
}

public interface IChannelObserver : IOrchestratorObserver
{
    //[OneWay]
    Task Join(AgentInfo agent);

    //[OneWay]
    Task Leave(AgentInfo agent);

    Task<ChatMsg?> GenerateReplyAsync(AgentInfo agent, ChatMsg[] msg);

    [OneWay]
    Task NewMessage(ChatMsg msg);
}

internal class ChannelObserver : IChannelObserver
{
    private readonly IAgent _agent;
    private readonly IChannelGrain _channelGrain;

    public ChannelObserver(IAgent agent, IChannelGrain channelGrain)
    {
        _agent = agent;
        _channelGrain = channelGrain;
    }

    public Task Join(AgentInfo agent)
    {
        if (agent.Name != _agent.Name)
        {
            Console.WriteLine($"Agent {_agent.Name} joined channel {_channelGrain.GetPrimaryKeyString()}");
        }
        return Task.CompletedTask;
    }

    public Task Leave(AgentInfo agent)
    {
        if (agent.Name != _agent.Name)
        {
            Console.WriteLine($"Agent {_agent.Name} left channel {_channelGrain.GetPrimaryKeyString()}");
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
        Console.WriteLine($"Notification from {_channelGrain.GetPrimaryKeyString()}: {msg.Text}");
        return Task.CompletedTask;
    }

    public Task NewMessage(ChatMsg msg)
    {
        Console.WriteLine($"Message from {msg.From}: {msg.Text}");
        return Task.CompletedTask;
    }
}

public class RoomObserver : IRoomObserver
{
    private readonly IAgent _agent;
    private readonly IClusterClient _client;
    private readonly IDictionary<string, IChannelObserver> _channelObservers = new Dictionary<string, IChannelObserver>();

    public RoomObserver(IAgent agent, IClusterClient client)
    {
        _agent = agent;
        _client = client;
    }

    public async Task AddMemberToChannel(ChannelInfo channel, AgentInfo agent)
    {
        if (agent.Name == _agent.Name)
        {
            var channelGrain = _client.GetGrain<IChannelGrain>(channel.Name);
            var channelObserver = new ChannelObserver(_agent, channelGrain);
            var reference = _client.CreateObjectReference<IChannelObserver>(channelObserver);
            _channelObservers[channel.Name] = reference;
            await channelGrain.Subscribe(reference);
            await channelGrain.Join(agent);
        }
    }

    public async Task RemoveMemberFromChannel(ChannelInfo channel, AgentInfo agent)
    {
        if (agent.Name == _agent.Name)
        {
            var channelGrain = _client.GetGrain<IChannelGrain>(channel.Name);
            var channelObserver = _channelObservers[channel.Name];
            await channelGrain.Unsubscribe(channelObserver);
            await channelGrain.Leave(agent);
            _channelObservers.Remove(channel.Name);
        }
    }

    public Task Notification(ChatMsg msg)
    {
        Console.WriteLine($"Message from {msg.From}: {msg.Text}");
        return Task.CompletedTask;
    }

    public Task Join(AgentInfo agent)
    {
        if (agent.Name != _agent.Name)
        {
            Console.WriteLine($"Agent {_agent.Name} joined the chat room");
        }

        return Task.CompletedTask;
    }

    public Task Leave(AgentInfo agent)
    {
        if (agent.Name != _agent.Name)
        {
            Console.WriteLine($"Agent {_agent.Name} left the chat room");
        }

        return Task.CompletedTask;
    }
}
