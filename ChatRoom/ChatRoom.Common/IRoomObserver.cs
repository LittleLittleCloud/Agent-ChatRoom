using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoGen.Core;
using Orleans.Runtime;

namespace ChatRoom.Common;

public interface IRoomObserver : IGrainObserver
{
    Task AddMemberToChannel(ChannelInfo channel, AgentInfo agent);

    Task RemoveMemberFromChannel(ChannelInfo channel, AgentInfo agent);

    Task Notification(ChatMsg msg);
}

public class RoomObserver : IRoomObserver
{
    private readonly IAgent _agent;
    private readonly IClusterClient _client;

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
            var observer = new OrchestratorStreamObserver(_agent, channelGrain);
            var streamProvider = _client.GetStreamProvider("chat");
            var stream = streamProvider.GetStream<AgentInfo>(StreamId.Create("AgentInfo", channel.Name));
            await stream.SubscribeAsync(observer);
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

            var handlers = await stream.GetAllSubscriptionHandles();
            foreach (var handler in handlers)
            {
                await handler.UnsubscribeAsync();
            }

            await channelGrain.Leave(agent);
        }
    }

    public Task Notification(ChatMsg msg)
    {
        Console.WriteLine($"Message from {msg.From}: {msg.Text}");
        return Task.CompletedTask;
    }
}
