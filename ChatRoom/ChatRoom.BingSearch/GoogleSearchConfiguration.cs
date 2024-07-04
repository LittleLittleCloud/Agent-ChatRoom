using System.Text.Json.Serialization;
using ChatRoom.OpenAI;
using ChatRoom.SDK;
using Json.Schema.Generation;

namespace ChatRoom.WebSearch;

public class GoogleSearchConfiguration
{
    [Description("Name of the google search agent, default is 'google-search'")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = "google-search";

    [Description("System message, default is 'You are a Google search agent. You can search the web using Google search engine.'")]
    [JsonPropertyName("system_message")]
    public string SystemMessage { get; set; } = "You are a Google search agent. You can search the web using Google search engine.";

    [Description("Agent description, default is 'I am a Google search agent. I can search the web using Google search engine.'")]
    [JsonPropertyName("description")]
    public string Description { get; set; } = "I am a Google search agent. I can search the web using Google search engine.";

    [Description("Google search API key, will use $env:GOOGLE_API_KEY if not provided")]
    [JsonPropertyName("google_api_key")]
    public string? GoogleApiKey { get; set; } = Environment.GetEnvironmentVariable("GOOGLE_API_KEY");

    [JsonPropertyName("google_custom_search_engine_id")]
    [Description("Google custom search engine id, will use $env:GOOGLE_CUSTOM_SEARCH_ENGINE_ID if not provided")]
    public string? GoogleCustomSearchEngineId { get; set; } = Environment.GetEnvironmentVariable("GOOGLE_CUSTOM_SEARCH_ENGINE_ID");

    [Description("OpenAI configuration")]
    [JsonPropertyName("openai_config")]
    public OpenAIClientConfiguration? OpenAIConfiguration { get; set; } = new OpenAIClientConfiguration();
}

