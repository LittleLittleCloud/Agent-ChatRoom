using System.Text.Json.Serialization;
using Json.Schema.Generation;

namespace ChatRoom.SDK;

public class RoomConfiguration
{
    [Description("The name of the room. Default is 'room'")]
    [JsonPropertyName("room")]
    public string Room { get; set; } = "room";

    [Description("The port number where the room is hosted. Default is 30000")]
    [JsonPropertyName("port")]
    public int Port { get; set; } = 30000;

    [Description("The timeout in seconds for the channel to wait for a response from its members. Default is 120 seconds.")]
    [JsonPropertyName("timeout")]
    public int Timeout { get; set; } = 120;
}
