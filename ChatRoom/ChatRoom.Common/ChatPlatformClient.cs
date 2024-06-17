using AutoGen.Core;

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
}
