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
        var observer = new AutoGenAgentObserver(_client, agent);
        var room = _client.GetGrain<IRoomGrain>(_room);
        var reference = _client.CreateObjectReference<IRoomObserver>(observer);
        await room.JoinRoom(agent.Name, description ?? string.Empty, false, reference);
        _observers[agent.Name] = reference;
    }

    public async Task UnregisterAgentAsync(IAgent agent)
    {
        var room = _client.GetGrain<IRoomGrain>(_room);
        await room.LeaveRoom(agent.Name);
        _observers.Remove(agent.Name);
    }
}
