using ChatRoom.SDK;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Orleans.Streams;
using Orleans.Utilities;

namespace ChatRoom.SDK;

internal class RoomGrain : Grain, IRoomGrain
{
    private readonly List<string> _channelNames = new(100);
    private readonly Dictionary<AgentInfo, IChatRoomAgentObserver> _agents = [];
    private readonly Dictionary<string, IOrchestratorObserver> _orchestrators = [];
    private readonly ILogger<RoomGrain>? _logger;

    public RoomGrain(
        ILogger<RoomGrain>? logger = null)
        : base()
    {
        _logger = logger;
        this.DelayDeactivation(TimeSpan.MaxValue);
    }

    public virtual string GrainKey => this.GetPrimaryKeyString();

    public Task<AgentInfo[]> GetMembers() => Task.FromResult(_agents.Keys.ToArray());

    public async Task AddAgentToRoom(string name, string description, bool isHuman, IChatRoomAgentObserver observer)
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

    public async Task RemoveAgentFromRoom(string nickname)
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
            await channelGrain.RemoveAgentFromChannel(agent.Name);
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
        ChatMsg[]? history = null,
        string[]? orchestrators = null)
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

        if (orchestrators is { Length: > 0 })
        {
            _logger?.LogInformation("Adding orchestrators to channel {ChannelName}", channelName);
            foreach (var orchestrator in orchestrators)
            {
                if (_orchestrators.All(x => x.Key != orchestrator))
                {
                    continue;
                }

                await AddOrchestratorToChannel(channelName, orchestrator);
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

        var channelGrain = this.GrainFactory.GetGrain<IChannelGrain>(channel);
        await channelGrain.ClearHistory();
        foreach (var orchestrator in _orchestrators)
        {
            await channelGrain.RemoveOrchestratorFromChannel(orchestrator.Key);
        }

        foreach (var agent in _agents)
        {
            await channelGrain.RemoveAgentFromChannel(agent.Key.Name);
        }

        await channelGrain.Delete();
        _channelNames.Remove(channel);
    }

    public async Task AddAgentToChannel(string channelName, string agentName)
    {
        var channel = _channelNames.First(x => x == channelName);
        var channelGrain = this.GrainFactory.GetGrain<IChannelGrain>(channel);
        var agent = _agents.First(x => x.Key.Name == agentName).Key;

        await channelGrain.AddAgentToChannel(agent.Name, agent.SelfDescription, agent.IsHuman, _agents[agent]);
    }

    public async Task RemoveAgentFromChannel(string channelName, string agentName)
    {
        var channel = _channelNames.First(x => x == channelName);
        var channelGrain = this.GrainFactory.GetGrain<IChannelGrain>(channel);
        var agent = _agents.First(x => x.Key.Name == agentName).Key;

        await channelGrain.RemoveAgentFromChannel(agent.Name);
    }

    public Task AddOrchestratorToRoom(string name, IOrchestratorObserver orchestrator)
    {
        _logger?.LogInformation("Adding orchestrator {OrchestratorName} to room {RoomName}", name, this.GrainKey);
        if (_orchestrators.ContainsKey(name))
        {
            return Task.CompletedTask;
        }

        _orchestrators[name] = orchestrator;
        return Task.CompletedTask;
    }

    public Task RemoveOrchestratorFromRoom(string name)
    {
        _logger?.LogInformation("Removing orchestrator {OrchestratorName} from room {RoomName}", name, this.GrainKey);
        if (!_orchestrators.ContainsKey(name))
        {
            return Task.CompletedTask;
        }

        _orchestrators.Remove(name);

        return Task.CompletedTask;
    }

    public Task AddOrchestratorToChannel(string channelName, string orchestratorName)
    {
        var channel = _channelNames.First(x => x == channelName);
        var channelGrain = this.GrainFactory.GetGrain<IChannelGrain>(channel);

        return channelGrain.AddOrchestratorToChannel(orchestratorName, _orchestrators[orchestratorName]);
    }

    public Task RemoveOrchestratorFromChannel(string channelName, string orchestratorName)
    {
        var channel = _channelNames.First(x => x == channelName);
        var channelGrain = this.GrainFactory.GetGrain<IChannelGrain>(channel);

        return channelGrain.RemoveOrchestratorFromChannel(orchestratorName);
    }

    public Task<string[]> GetOrchestrators()
    {
        return Task.FromResult(_orchestrators.Keys.ToArray());
    }

    public async Task CloneChannel(string channelName, string newChannelName)
    {
        if (_channelNames.Any(x => x == newChannelName))
        {
            _logger?.LogWarning("Channel {ChannelName} already exists", newChannelName);

            return;
        }
        var oldChannel = _channelNames.First(x => x == channelName);
        var oldChannelGrain = this.GrainFactory.GetGrain<IChannelGrain>(oldChannel);
        var oldChannelInfo = await oldChannelGrain.GetChannelInfo();
        var chatHistory = await oldChannelGrain.ReadHistory(1_000);
        await this.DeleteChannel(newChannelName);
        await this.CreateChannel(newChannelName, oldChannelInfo.Members.Select(m => m.Name).ToArray(), chatHistory);

        foreach (var orchestrator in oldChannelInfo.Orchestrators)
        {
            await this.AddOrchestratorToChannel(newChannelName, orchestrator);
        }
    }
}
