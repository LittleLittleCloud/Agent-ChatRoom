using ChatRoom.Common;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Orleans.Streams;
using Orleans.Utilities;

namespace ChatRoom.Room;

public class RoomGrain : Grain, IRoomGrain
{
    private readonly List<ChannelInfo> _channels = new(100);
    private readonly Dictionary<AgentInfo, IRoomObserver> _agents = [];
    private readonly ILogger<RoomGrain> _logger;

    public RoomGrain(ILogger<RoomGrain> logger)
    {
        _logger = logger;
    }

    public Task<AgentInfo[]> GetMembers() => Task.FromResult(_agents.Keys.ToArray());

    public async Task Join(AgentInfo nickname, IRoomObserver observer)
    {
        if (_agents.ContainsKey(nickname))
        {
            return;
        }

        var agentObserver = new ObserverManager<IRoomObserver>(TimeSpan.FromMinutes(1), _logger);
        agentObserver.Subscribe(observer, observer);
        _agents[nickname] = observer;

        foreach (var ob in _agents.Values)
        {
            await ob.Join(nickname, this.GetPrimaryKeyString());
        }
    }

    public async Task Leave(string nickname)
    {
        if (!_agents.Any(kv => kv.Key.Name == nickname))
        {
            return;
        }

        var agent = _agents.First(kv => kv.Key.Name == nickname).Key;
        _agents.Remove(agent);

        foreach (var ob in _agents.Values)
        {
            await ob.Leave(agent, this.GetPrimaryKeyString());
        }

        // remove agent from all channels
        foreach (var channel in _channels)
        {
            var channelGrain = this.GrainFactory.GetGrain<IChannelGrain>(channel.Name);
            await channelGrain.Leave(agent);
        }
    }

    public Task<ChannelInfo[]> GetChannels()
    {
        return Task.FromResult(_channels.ToArray());
    }

    public async Task CreateChannel(ChannelInfo channelInfo)
    {
        if (_channels.Any(x => x.Name == channelInfo.Name))
        {
            return;
        }

        _channels.Add(channelInfo);
    }

    public async Task DeleteChannel(string channelName)
    {
        if (_channels.All(x => x.Name != channelName))
        {
            return;
        }

        var channel = _channels.First(x => x.Name == channelName);
        _channels.Remove(channel);
    }

    public async Task AddAgentToChannel(ChannelInfo channelInfo, AgentInfo agentInfo)
    {
        var channel = _channels.First(x => x.Name == channelInfo.Name);
        var channelGrain = this.GrainFactory.GetGrain<IChannelGrain>(channel.Name);
        var agent = _agents.First(x => x.Key.Name == agentInfo.Name).Key;

        await channelGrain.Join(agent, _agents[agent]);
    }

    public async Task RemoveAgentFromChannel(ChannelInfo channelInfo, AgentInfo agentInfo)
    {
        var channel = _channels.First(x => x.Name == channelInfo.Name);
        var channelGrain = this.GrainFactory.GetGrain<IChannelGrain>(channel.Name);
        var agent = _agents.First(x => x.Key.Name == agentInfo.Name).Key;

        await channelGrain.Leave(agent);
    }
}
