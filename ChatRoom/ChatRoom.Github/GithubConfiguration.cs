using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Json.Schema.Generation;

namespace ChatRoom.Github;

public enum LLMType
{
    [Description("Azure OpenAI")]
    AOAI = 0, // Azure OpenAI

    [Description("OpenAI")]
    OpenAI = 1 // OpenAI
}

public class IssueHelperConfiguration
{
    [Description("Name of the issue helper agent, default is 'issue-helper'")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = "issue-helper";

    [Description("System message, default is 'You are a github issue helper'")]
    [JsonPropertyName("system_message")]
    public string SystemMessage { get; set; } = "You are a github issue helper";

    [Description("Agent description, default is 'I am a github issue helper, I can help you with your github issues.'")]
    [JsonPropertyName("description")]
    public string Description { get; set; } = "I am a github issue helper, I can help you with your github issues.";
}

public class GithubConfiguration
{
    [Description("Issue helper configuration")]
    public IssueHelperConfiguration IssueHelper { get; set; } = new IssueHelperConfiguration();

    [JsonPropertyName("github_token")]
    [Description("GitHub token, will use $env:GITHUB_TOKEN if not provided")]
    public string? GithubToken { get; set; } = Environment.GetEnvironmentVariable("GITHUB_TOKEN");

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
