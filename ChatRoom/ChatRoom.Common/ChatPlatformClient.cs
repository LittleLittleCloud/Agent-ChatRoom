using AutoGen.Core;
using Orleans.Runtime;
using Orleans;

namespace ChatRoom.SDK;

public class ChatPlatformClient
{
    private readonly IClusterClient _client;
    private readonly string _room;
    private readonly IDictionary<string, IRoomObserver> _observers = new Dictionary<string, IRoomObserver>();
    private readonly IDictionary<string, IOrchestratorObserver> _orchestrators = new Dictionary<string, IOrchestratorObserver>();

    public ChatPlatformClient(IClusterClient client, string room = "room")
    {
        _client = client;
        _room = room;
    }

    public async Task RegisterAgentAsync(IAgent agent, string? description = null)
    {
        var observer = new AutoGenAgentObserver(_client, agent);
        var room = _client.GetGrain<IRoomGrain>(_room);
        var reference = _client.CreateObjectReference<IRoomObserver>(observer);
        await room.AddAgentToRoom(agent.Name, description ?? string.Empty, false, reference);
        _observers[agent.Name] = observer;
    }

    public async Task UnregisterAgentAsync(IAgent agent)
    {
        var room = _client.GetGrain<IRoomGrain>(_room);
        await room.RemoveAgentFromRoom(agent.Name);
        _observers.Remove(agent.Name);
    }

    public async Task RegisterOrchestratorAsync(string name, IOrchestrator orchestrator)
    {
        var observer = new OrchestratorObserver(orchestrator);
        await this.RegisterOrchestratorAsync(name, observer);
    }

    public async Task RegisterOrchestratorAsync(string name, IOrchestratorObserver observer)
    {
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

    public async Task<ChannelInfo> GetChannelInfo(string channelName)
    {
        var channel = _client.GetGrain<IChannelGrain>(channelName);

        return await channel.GetChannelInfo();
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
        ChatMsg[]? chatHistory = null)
    {
        var room = _client.GetGrain<IRoomGrain>(_room);
        await room.CreateChannel(channelName, members, chatHistory);
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
