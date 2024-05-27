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

    public RoomGrain(
        ILogger<RoomGrain> logger)
    {
        _logger = logger;
    }
    public new virtual IGrainFactory GrainFactory => base.GrainFactory;

    public virtual string GrainKey => this.GetPrimaryKeyString(); 

    public Task<AgentInfo[]> GetMembers() => Task.FromResult(_agents.Keys.ToArray());

    public async Task JoinRoom(string name, string description, bool isHuman, IRoomObserver observer)
    {
        var agent = new AgentInfo(name, description, isHuman);
        if (_agents.ContainsKey(agent))
        {
            return;
        }

        var agentObserver = new ObserverManager<IRoomObserver>(TimeSpan.FromMinutes(1), _logger);
        agentObserver.Subscribe(observer, observer);
        _agents[agent] = observer;

        foreach (var ob in _agents.Values)
        {
            await ob.JoinRoom(agent, this.GrainKey);
        }
    }

    public async Task LeaveRoom(string nickname)
    {
        if (!_agents.Any(kv => kv.Key.Name == nickname))
        {
            return;
        }

        var agent = _agents.First(kv => kv.Key.Name == nickname).Key;
        _agents.Remove(agent);

        foreach (var ob in _agents.Values)
        {
            await ob.LeaveRoom(agent, this.GrainKey);
        }

        // remove agent from all channels
        foreach (var channel in _channels)
        {
            var channelGrain = this.GrainFactory.GetGrain<IChannelGrain>(channel.Name);
            await channelGrain.LeaveChannel(agent.Name);
        }
    }

    public Task<ChannelInfo[]> GetChannels()
    {
        return Task.FromResult(_channels.ToArray());
    }

    public async Task CreateChannel(
        string channelName,
        string[]? members = null,
        ChatMsg[]? history = null)
    {
        if (_channels.Any(x => x.Name == channelName))
        {
            _logger.LogWarning("Channel {ChannelName} already exists", channelName);
            return;
        }

        var channelInfo = new ChannelInfo(channelName);

        _channels.Add(channelInfo);

        var channelGrain = this.GrainFactory.GetGrain<IChannelGrain>(channelName);
        if (history is { Length: > 0 })
        {
            _logger.LogInformation("Initializing chat history for channel {ChannelName}", channelName);
            await channelGrain.InitializeChatHistory(history);
        }

        if (members is { Length: > 0 })
        {
            _logger.LogInformation("Adding members to channel {ChannelName}", channelName);
            foreach (var member in members)
            {
                await AddAgentToChannel(channelInfo, member);
            }
        }

        _logger.LogInformation("Channel {ChannelName} created", channelName);
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

    public async Task AddAgentToChannel(ChannelInfo channelInfo, string agentName)
    {
        var channel = _channels.First(x => x.Name == channelInfo.Name);
        var channelGrain = this.GrainFactory.GetGrain<IChannelGrain>(channel.Name);
        var agent = _agents.First(x => x.Key.Name == agentName).Key;

        await channelGrain.JoinChannel(agent.Name, agent.SelfDescription, agent.IsHuman, _agents[agent]);
    }

    public async Task RemoveAgentFromChannel(ChannelInfo channelInfo, string agentName)
    {
        var channel = _channels.First(x => x.Name == channelInfo.Name);
        var channelGrain = this.GrainFactory.GetGrain<IChannelGrain>(channel.Name);
        var agent = _agents.First(x => x.Key.Name == agentName).Key;

        await channelGrain.LeaveChannel(agent.Name);
    }
}
