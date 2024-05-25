using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Json.Schema.Generation;

namespace ChatRoom.BingSearch;

public enum LLMType
{
    [Description("Azure OpenAI")]
    AOAI = 0, // Azure OpenAI

    [Description("OpenAI")]
    OpenAI = 1 // OpenAI
}

public class BingSearchConfiguration
{
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

    [Description("Language model type, will use Azure OpenAI if not provided")]
    [JsonPropertyName("llm_type")]
    public LLMType LLMType { get; set; } = LLMType.AOAI;

    [JsonPropertyName("azure_openai_endpoint")]
    [Description("Azure OpenAI endpoint, will use $env:AZURE_OPENAI_ENDPOINT if not provided")]
    public string? AzureOpenAiEndpoint { get; set; } = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");

    [JsonPropertyName("azure_openai_key")]
    [Description("Azure OpenAI key, will use $env:AZURE_OPENAI_API_KEY if not provided")]
    public string? AzureOpenAiKey { get; set; } = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");

    [JsonPropertyName("azure_deployment_name")]
    [Description("Azure OpenAI deployment name, will use $env:AZURE_OPENAI_DEPLOY_NAME if not provided")]
    public string? AzureDeploymentName { get; set; } = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOY_NAME");


    [JsonPropertyName("openai_api_key")]
    [Description("OpenAI API key, will use $env:OPENAI_API_KEY if not provided")]
    public string? OpenAiApiKey { get; set; } = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

    [JsonPropertyName("openai_model_id")]
    [Description("OpenAI model ID, will use gpt-3.5-turbo if not provided")]
    public string OpenAiModelId { get; set; } = "gpt-3.5-turbo";
}
