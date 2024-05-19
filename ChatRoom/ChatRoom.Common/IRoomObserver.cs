using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using AutoGen.Core;
using Orleans.Runtime;

namespace ChatRoom.Common;

public interface INotificationObserver : IGrainObserver
{
    Task Notification(ChatMsg msg);
}

public interface IRoomObserver : INotificationObserver
{
    Task Join(AgentInfo agent);

    Task Leave(AgentInfo agent);

    Task AddMemberToChannel(ChannelInfo channel, AgentInfo agent);

    Task RemoveMemberFromChannel(ChannelInfo channel, AgentInfo agent);
}

public interface IOrchestratorObserver : INotificationObserver
{
    Task NextSpeaker(AgentInfo agent);
}

public interface IChannelObserver : IOrchestratorObserver
{
    Task Join(AgentInfo agent);

    Task Leave(AgentInfo agent);

    Task NewMessage(ChatMsg msg);
}

public class ChannelObserver : IChannelObserver
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

    public Task NewMessage(ChatMsg msg)
    {
        Console.WriteLine($"Message from {msg.From}: {msg.Text}");
        return Task.CompletedTask;
    }

    public async Task NextSpeaker(AgentInfo agent)
    {
        if (agent.Name != _agent.Name)
        {
            return;
        }

        var messages = await _channelGrain.ReadHistory(10);

        // convert ChatMsg to TextMessage
        var textMessages = messages.Select(msg => new TextMessage(Role.Assistant, msg.Text, from: msg.From)).ToArray();
        var reply = await _agent!.GenerateReplyAsync(textMessages);

        if (reply.GetContent() is string content)
        {
            var returnMessage = new ChatMsg(_agent.Name, content);
            await _channelGrain.Message(returnMessage);

            return;
        }
    }

    public Task Notification(ChatMsg msg)
    {
        Console.WriteLine($"Notification from {_channelGrain.GetPrimaryKeyString()}: {msg.Text}");
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
            var members = await channelGrain.GetMembers();
            if (members.Any(m => m.Name == agent.Name))
            {
                Console.WriteLine($"Agent {agent.Name} is already in channel {channel.Name}");
                return;
            }

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
            var members = await channelGrain.GetMembers();
            if (!members.Any(m => m.Name == agent.Name))
            {
                Console.WriteLine($"Agent {agent.Name} is not in channel {channel.Name}");
                return;
            }

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
