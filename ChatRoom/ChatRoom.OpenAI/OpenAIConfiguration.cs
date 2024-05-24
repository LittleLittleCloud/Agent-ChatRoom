using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ChatRoom.OpenAI;

public class OpenAIConfiguration
{
    [JsonPropertyName("use_aoai")]
    public bool UseAOAI { get; set; } = false;

    [JsonPropertyName("openai_api_key")]
    public string? OpenAIApiKey { get; set; }

    [JsonPropertyName("openai_model_id")]
    public string? OpenAIModelId { get; set; }

    [JsonPropertyName("azure_openai_endpoint")]
    public string? AzureOpenAIEndpoint { get; set; }

    [JsonPropertyName("azure_openai_key")]
    public string? AzureOpenAIKey { get; set; }

    [JsonPropertyName("azure_openai_deploy_name")]
    public string? AzureOpenAIDeployName { get; set; }

    [JsonPropertyName("room")]
    public string Room { get; init; } = null!;

    [JsonPropertyName("port")]
    public int Port { get; init; } = 30000;

    [JsonPropertyName("system_message")]
    public string SystemMessage { get; init; } = "You are a helpful AI assistant";

    [JsonPropertyName("name")]
    public string Name { get; init; } = "gpt";
}
