using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ChatRoom.OpenAI;
using ChatRoom.SDK;
using Json.Schema.Generation;

namespace ChatRoom.BingSearch;

public class BingSearchConfiguration
{
    [JsonPropertyName("room_config")]
    [Description("The configuration for the chat room")]
    public RoomConfiguration RoomConfig { get; set; } = new RoomConfiguration();

    [Description("Name of the bing search agent, default is 'bing-search'")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = "bing-search";

    [Description("System message, default is 'You are a Bing search agent. You can search the web using Bing search engine.'")]
    [JsonPropertyName("system_message")]
    public string SystemMessage { get; set; } = "You are a Bing search agent. You can search the web using Bing search engine.";

    [Description("Agent description, default is 'I am a Bing search agent. I can search the web using Bing search engine.'")]
    [JsonPropertyName("description")]
    public string Description { get; set; } = "I am a Bing search agent. I can search the web using Bing search engine.";

    [Description("Bing API key, will use $env:BING_API_KEY if not provided")]
    [JsonPropertyName("bing_api_key")]
    public string? BingApiKey { get; set; } = Environment.GetEnvironmentVariable("BING_API_KEY");

    [Description("OpenAI configuration")]
    [JsonPropertyName("openai_config")]
    public OpenAIClientConfiguration? OpenAIConfiguration { get; set; } = null;
}
