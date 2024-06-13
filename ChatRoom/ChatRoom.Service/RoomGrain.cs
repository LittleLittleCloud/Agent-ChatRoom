using ChatRoom.Common;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Orleans.Streams;
using Orleans.Utilities;

namespace ChatRoom.Room;

public class RoomGrain : Grain, IRoomGrain
{
    private readonly List<string> _channelNames = new(100);
    private readonly Dictionary<AgentInfo, IRoomObserver> _agents = [];
    private readonly ILogger<RoomGrain>? _logger;

    public RoomGrain(
        ILogger<RoomGrain>? logger = null)
        :base()
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
        foreach (var channel in _channelNames)
        {
            var channelGrain = this.GrainFactory.GetGrain<IChannelGrain>(channel);
            await channelGrain.LeaveChannel(agent.Name);
        }
    }

    public async Task<ChannelInfo[]> GetChannels()
    {
        var channelInfos = new List<ChannelInfo>();
        foreach (var channelName in _channelNames)
        {
            var channelGrain = this.GrainFactory.GetGrain<IChannelGrain>(channelName);
            var channelInfo = await channelGrain.GetChannelInfo();
            channelInfos.Add(channelInfo);
        }

        return channelInfos.ToArray();
    }

    public async Task CreateChannel(
        string channelName,
        string[]? members = null,
        ChatMsg[]? history = null)
    {
        if (_channelNames.Any(x => x == channelName))
        {
            _logger?.LogWarning("Channel {ChannelName} already exists", channelName);
            return;
        }

        // activate the channel
        _channelNames.Add(channelName);

        var channelGrain = this.GrainFactory.GetGrain<IChannelGrain>(channelName);
        if (history is { Length: > 0 })
        {
            _logger?.LogInformation("Initializing chat history for channel {ChannelName}", channelName);
            await channelGrain.InitializeChatHistory(history);
        }

        if (members is { Length: > 0 })
        {
            _logger?.LogInformation("Adding members to channel {ChannelName}", channelName);
            foreach (var member in members)
            {
                if (_agents.All(x => x.Key.Name != member))
                {
                    continue;
                }

                await AddAgentToChannel(channelName, member);
            }
        }

        _logger?.LogInformation("Channel {ChannelName} created", channelName);
    }

    public async Task DeleteChannel(string channelName)
    {
        if (_channelNames.All(x => x != channelName))
        {
            return;
        }

        var channel = _channelNames.First(x => x == channelName);
        _channelNames.Remove(channel);
    }

    public async Task AddAgentToChannel(string channelName, string agentName)
    {
        var channel = _channelNames.First(x => x == channelName);
        var channelGrain = this.GrainFactory.GetGrain<IChannelGrain>(channel);
        var agent = _agents.First(x => x.Key.Name == agentName).Key;

        await channelGrain.JoinChannel(agent.Name, agent.SelfDescription, agent.IsHuman, _agents[agent]);
    }

    public async Task RemoveAgentFromChannel(string channelName, string agentName)
    {
        var channel = _channelNames.First(x => x == channelName);
        var channelGrain = this.GrainFactory.GetGrain<IChannelGrain>(channel);
        var agent = _agents.First(x => x.Key.Name == agentName).Key;

        await channelGrain.LeaveChannel(agent.Name);
    }
}
