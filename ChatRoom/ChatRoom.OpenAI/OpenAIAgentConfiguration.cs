using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Json.Schema.Generation;
using Spectre.Console.Cli;

namespace ChatRoom.OpenAI;

public class OpenAIAgentConfiguration
{
    [Description("Use AOAI, default is true")]
    [JsonPropertyName("use_aoai")]
    public bool UseAOAI { get; set; } = true;

    [Description("OpenAI API key, default is $env:OPENAI_API_KEY")]
    [JsonPropertyName("openai_api_key")]
    public string? OpenAIApiKey { get; set; } = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

    [Description("OpenAI model ID, default is gpt-3.5-turbo")]
    [JsonPropertyName("openai_model_id")]
    public string? OpenAIModelId { get; set; } = "gpt-3.5-turbo";

    [Description("Azure OpenAI endpoint, default is $env:AZURE_OPENAI_ENDPOINT")]
    [JsonPropertyName("azure_openai_endpoint")]
    public string? AzureOpenAIEndpoint { get; set; } = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");

    [Description("Azure OpenAI key, default is $env:AZURE_OPENAI_API_KEY")]
    [JsonPropertyName("azure_openai_key")]
    public string? AzureOpenAIKey { get; set; } = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");

    [Description("Azure OpenAI deploy name, default is $env:AZURE_OPENAI_DEPLOY_NAME")]
    [JsonPropertyName("azure_openai_deploy_name")]
    public string? AzureOpenAIDeployName { get; set; } = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOY_NAME");

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
