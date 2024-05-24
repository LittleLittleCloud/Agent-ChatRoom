using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Spectre.Console.Cli;

namespace ChatRoom.OpenAI;

public class OpenAIAgentConfiguration
{
    [JsonPropertyName("use_aoai")]
    public bool UseAOAI { get; set; } = true;

    [JsonPropertyName("openai_api_key")]
    public string? OpenAIApiKey { get; set; } = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

    [JsonPropertyName("openai_model_id")]
    public string? OpenAIModelId { get; set; } = "gpt-3.5-turbo";

    [JsonPropertyName("azure_openai_endpoint")]
    public string? AzureOpenAIEndpoint { get; set; } = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");

    [JsonPropertyName("azure_openai_key")]
    public string? AzureOpenAIKey { get; set; } = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");

    [JsonPropertyName("azure_openai_deploy_name")]
    public string? AzureOpenAIDeployName { get; set; } = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOY_NAME");

    [JsonPropertyName("system_message")]
    public string SystemMessage { get; init; } = "You are a helpful AI assistant";

    [JsonPropertyName("name")]
    public string Name { get; init; } = "gpt";
}
