using System.Text.Json.Serialization;
using Json.Schema.Generation;

namespace ChatRoom.SDK;

public class ChannelConfiguration
{
    [Description("openai configuration, this will be used to create openai client")]
    [JsonPropertyName("openai_config")]
    public OpenAIClientConfiguration OpenAIConfiguration { get; set; } = new OpenAIClientConfiguration();
}
