using System.Text.Json.Serialization;
using ChatRoom.SDK;
using Json.Schema.Generation;

namespace ChatRoom.OpenAI;

public class OpenAIAgentConfiguration
{
    [Description("llm configuration")]
    [JsonPropertyName("llm_config")]
    public OpenAIClientConfiguration LLMConfiguration { get; set; } = new OpenAIClientConfiguration();

    [Description("System message used in gpt agent, default is 'You are a helpful AI assistant'")]
    [JsonPropertyName("system_message")]
    public string SystemMessage { get; init; } = "You are a helpful AI assistant";

    [Description("Agent description used in gpt agent, default is 'I am a helpful AI assistant'")]
    [JsonPropertyName("agent_description")]
    public string Description { get; init; } = "I am a helpful AI assistant";

    [Description("Name of the agent, default is 'gpt'")]
    [JsonPropertyName("name")]
    public string Name { get; init; } = "gpt";
}
