using AutoGen.Core;

namespace ChatRoom.Common;

public class ChatPlatformClient
{
    private readonly IClusterClient _client;
    private readonly string _room;
    private readonly IDictionary<string, IRoomObserver> _observers = new Dictionary<string, IRoomObserver>();

    public ChatPlatformClient(IClusterClient client, string room = "room")
    {
        _client = client;
        _room = room;
    }

    public async Task RegisterAgentAsync(IAgent agent, string? description = null)
    {
        var room = _client.GetGrain<IRoomGrain>(_room);
        var agentInfo = new AgentInfo(agent.Name, description ?? string.Empty, false);
        await room.Join(agentInfo);
        var observer = new RoomObserver(agent, _client);
        var reference = _client.CreateObjectReference<IRoomObserver>(observer);
        _observers[agent.Name] = reference;
        await room.Subscribe(reference);
    }

    public async Task UnregisterAgentAsync(IAgent agent)
    {
        var room = _client.GetGrain<IRoomGrain>(_room);

        var observer = _observers[agent.Name];
        await room.Leave(agent.Name);
        //await room.Unsubscribe(observer);
        _observers.Remove(agent.Name);
    }
}
