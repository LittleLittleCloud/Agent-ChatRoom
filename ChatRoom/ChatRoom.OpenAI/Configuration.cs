using System.Text.Json.Serialization;
using ChatRoom.SDK;
using Json.Schema.Generation;

namespace ChatRoom.OpenAI;

internal class Configuration
{
    [Description("The configuration for the chat room")]
    [JsonPropertyName("room_config")]
    public RoomConfiguration RoomConfig { get; set; } = new RoomConfiguration();

    [Description("agents, the name of agents can't be duplicated")]
    [JsonPropertyName("agents")]
    public List<OpenAIAgentConfiguration> Agents { get; set; } = new List<OpenAIAgentConfiguration>();
}
