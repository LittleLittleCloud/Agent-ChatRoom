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
