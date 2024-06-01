using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ChatRoom.OpenAI;
using Json.Schema.Generation;

namespace ChatRoom.Room;

public class ChannelConfiguration
{
    [Description("openai configuration, this will be used to create openai client")]
    [JsonPropertyName("openai_config")]
    public OpenAIClientConfiguration OpenAIConfiguration { get; set; } = new OpenAIClientConfiguration();
}

public class RoomConfiguration
{
    [Description("The name of the room. Default is 'room'")]
    [JsonPropertyName("room")]
    public string Room { get; set; } = "room";

    [Description("The port number where the room is hosted. Default is 30000")]
    [JsonPropertyName("port")]
    public int Port { get; set; } = 30000;
}
