using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ChatRoom.Room;

public class ChannelConfiguration
{
    [JsonPropertyName("room")]
    public RoomConfiguration RoomConfig { get; set; } = new RoomConfiguration();

    [JsonPropertyName("use_aoai")]
    public bool UseAOAI { get; set; } = true;

    [JsonPropertyName("openai_api_key")]
    public string? OpenAIApiKey { get; set; } = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

    [JsonPropertyName("openai_model_id")]
    public string? OpenAIModelId { get; set; } = "gpt-3.5-turbo";

    [JsonPropertyName("azure_openai_endpoint")]
    public string? AzureOpenAIEndpoint { get; set; } = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");

    [JsonPropertyName("azure_openai_key")]
    public string? AzureOpenAIKey { get; set; } = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY");

    [JsonPropertyName("azure_openai_deploy_name")]
    public string? AzureOpenAIDeployName { get; set; } = Environment.GetEnvironmentVariable("AZURE_DEPLOYMENT_NAME");
}

public class RoomConfiguration
{
    [JsonPropertyName("room")]
    public string Room { get; set; } = "room";

    [JsonPropertyName("port")]
    public int Port { get; set; } = 30000;
}
