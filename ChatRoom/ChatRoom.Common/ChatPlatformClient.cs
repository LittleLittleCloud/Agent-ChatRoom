using AutoGen.Core;
using Orleans.Runtime;
using Orleans;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ChatRoom.SDK.Orleans;
using System.Reflection;

namespace ChatRoom.SDK;

public class ChatPlatformClient
{
    private readonly IClusterClient _client;
    private readonly string _room;
    private readonly IHostApplicationLifetime? _lifetime;
    private readonly IDictionary<string, IChatRoomAgentObserver> _observers = new Dictionary<string, IChatRoomAgentObserver>();
    private readonly IDictionary<string, IOrchestratorObserver> _orchestrators = new Dictionary<string, IOrchestratorObserver>();
    private readonly ILogger<ChatPlatformClient>? _logger;
    
    public ChatPlatformClient(
        IClusterClient client,
        string room = "room",
        IHostApplicationLifetime? lifecycleService = null,
        ILogger<ChatPlatformClient>? logger = null)
    {
        _client = client;
        _room = room;
        _lifetime = lifecycleService;
        _logger = logger;
    }

    public async Task RegisterAutoGenAgentAsync(IAgent agent, string? description = null)
    {
        var observer = new AutoGenAgentObserver(_client, agent);
        await this.RegisterAgentAsync(agent.Name, description ?? string.Empty, false, observer);
    }

    public async Task UnregisterAgentAsync(string name)
    {
        _logger?.LogInformation($"Unregistering agent {name}");
        var room = _client.GetGrain<IRoomGrain>(_room);
        await room.RemoveAgentFromRoom(name);
        _observers.Remove(name);
    }

    public async Task RegisterAgentAsync(string name, string description, bool isHuman, IChatRoomAgent agent)
    {
        _logger?.LogInformation($"Registering agent {name}");
        var room = _client.GetGrain<IRoomGrain>(_room);
        var observer = new ChatRoomAgentObserver(agent);
        var reference = _client.CreateObjectReference<IChatRoomAgentObserver>(observer);
        await room.AddAgentToRoom(name, description, isHuman, reference);
        _observers[name] = observer;
        
        if (_lifetime is not null)
        {
            _lifetime.ApplicationStopping.Register(async() => await this.UnregisterAgentAsync(name));
        }
    }

    public async Task RegisterAutoGenGroupChatAsync(string name, GroupChat groupChat)
    {
        _logger?.LogInformation($"Registering group chat {name}");

        // register agents
        // use reflection to get groupChat.agents
        var agents = groupChat.GetType().GetField("agents", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(groupChat) as List<IAgent> ?? new List<IAgent>();

        // register agents if not already registered
        foreach (var agent in agents)
        {
            if (!_observers.ContainsKey(agent.Name))
            {
                await this.RegisterAutoGenAgentAsync(agent);
            }
        }

        // use reflection to get groupChat.orchestrator
        var orchestrator = groupChat.GetType().GetField("orchestrator", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(groupChat) as AutoGen.Core.IOrchestrator;
        // register orchestrator if not already registered
        if (orchestrator is not null && !_orchestrators.ContainsKey(name))
        {
            await this.RegisterAutoGenOrchestratorAsync(name, orchestrator);
        }

        // create a channel for the group chat if not already created
        if (await this.GetChannelInfo(name) is null)
        {
            await this.CreateChannel(name, agents.Select(a => a.Name).ToArray(), [], [name]);
        }
        else
        {
            this._logger?.LogWarning($"Channel {name} already exists");
        }
    }

    public async Task RegisterAutoGenOrchestratorAsync(string name, AutoGen.Core.IOrchestrator orchestrator)
    {
        var observer = new AutoGenOrchestratorObserver(orchestrator);
        await this.RegisterOrchestratorAsync(name, observer);
    }

    public async Task RegisterOrchestratorAsync(string name, IOrchestrator orchestrator)
    {
        var observer = new OrchestratorObserver(orchestrator);
        var room = _client.GetGrain<IRoomGrain>(_room);
        var reference = _client.CreateObjectReference<IOrchestratorObserver>(observer);
        await room.AddOrchestratorToRoom(name, reference);
        _orchestrators[name] = observer;
    }

    public async Task UnregisterOrchestratorAsync(string name)
    {
        var room = _client.GetGrain<IRoomGrain>(_room);
        await room.RemoveOrchestratorFromRoom(name);
        _orchestrators.Remove(name);
    }

    public async Task<string[]> GetOrchestrators()
    {
        var room = _client.GetGrain<IRoomGrain>(_room);
        return await room.GetOrchestrators();
    }

    public async Task<ChannelInfo?> GetChannelInfo(string channelName)
    {
        var channels = await this.GetChannels();
        return channels.FirstOrDefault(c => c.Name == channelName);
    }

    public async Task AddOrchestratorToChannel(string channelName, string orchestratorName)
    {
        var room = _client.GetGrain<IRoomGrain>(_room);
        await room.AddOrchestratorToChannel(channelName, orchestratorName);
    }

    public async Task RemoveOrchestratorFromChannel(string channelName, string orchestratorName)
    {
        var room = _client.GetGrain<IRoomGrain>(_room);
        await room.RemoveOrchestratorFromChannel(channelName, orchestratorName);
    }

    public async Task CloneChannel(string channelName, string newChannelName)
    {
        var room = _client.GetGrain<IRoomGrain>(_room);
        await room.CloneChannel(channelName, newChannelName);
    }

    public async Task EditChannelName(string oldChannelName, string newChannelName)
    {
        var room = _client.GetGrain<IRoomGrain>(_room);
        await room.CloneChannel(oldChannelName, newChannelName);
        await room.DeleteChannel(oldChannelName);
    }

    public async Task DeleteChannel(string channelName)
    {
        var room = _client.GetGrain<IRoomGrain>(_room);
        await room.DeleteChannel(channelName);
    }

    public async Task ClearChannelHistory(string channelName)
    {
        var channel = _client.GetGrain<IChannelGrain>(channelName);
        await channel.ClearHistory();
    }

    public async Task SendMessageToChannel(string channelName, ChatMsg msg)
    {
        var channel = _client.GetGrain<IChannelGrain>(channelName);
        await channel.SendMessage(msg);
    }

    public async Task<ChatMsg?> GenerateNextReply(string channelName, string[]? candidates = null, ChatMsg[]? chatMsgs = null, string? orchestrator = null)
    {
        var room = _client.GetGrain<IRoomGrain>(_room);
        var channel = await room.GetChannels();


        var channelGrain = _client.GetGrain<IChannelGrain>(channelName);
        return await channelGrain.GenerateNextReply(candidates, chatMsgs, orchestrator: orchestrator);
    }

    public async Task<IEnumerable<AgentInfo>> GetRoomMembers()
    {
        var room = _client.GetGrain<IRoomGrain>(_room);
        return await room.GetMembers();
    }

    public async Task<IEnumerable<ChannelInfo>> GetChannels()
    {
        var room = _client.GetGrain<IRoomGrain>(_room);
        return await room.GetChannels();
    }

    public async Task<IEnumerable<AgentInfo>> GetChannelMembers(string channelName)
    {
        var channel = _client.GetGrain<IChannelGrain>(channelName);
        return await channel.GetMembers();
    }

    public async Task<IEnumerable<ChatMsg>> GetChannelChatHistory(string channelName, int count = 1_000)
    {
        var channel = _client.GetGrain<IChannelGrain>(channelName);
        return await channel.ReadHistory(count);
    }

    public async Task CreateChannel(
        string channelName,
        string[]? members = null,
        ChatMsg[]? chatHistory = null,
        string[]? orchestrators = null)
    {
        var room = _client.GetGrain<IRoomGrain>(_room);
        await room.CreateChannel(channelName, members, chatHistory, orchestrators);
    }

    public async Task AddAgentToChannel(string channelName, string agentName)
    {
        var room = _client.GetGrain<IRoomGrain>(_room);
        await room.AddAgentToChannel(channelName, agentName);
    }

    public async Task RemoveAgentFromChannel(string channelName, string agentName)
    {
        var room = _client.GetGrain<IRoomGrain>(_room);
        await room.RemoveAgentFromChannel(channelName, agentName);
    }
}
