using System.Text.Json.Serialization;
using ChatRoom.Room;

namespace ChatRoom.Client;

public class ChatRoomClientConfiguration
{
    [JsonPropertyName("room_config")]
    public RoomConfiguration RoomConfig { get; set; } = new RoomConfiguration();

    [JsonPropertyName("channel_config")]
    public ChannelConfiguration ChannelConfig { get; set; } = new ChannelConfiguration();

    [JsonPropertyName("agent_extensions")]
    public List<AgentExtensionConfiguration> AgentExtensions { get; set; } = [];

    [JsonPropertyName("port")]
    public int Port { get; set; } = 30000;

    [JsonPropertyName("name")]
    public string YourName { get; set; } = "User";
}

public class AgentExtensionConfiguration
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;
}
