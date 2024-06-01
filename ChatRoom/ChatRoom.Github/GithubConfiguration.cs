using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ChatRoom.OpenAI;
using Json.Schema.Generation;

namespace ChatRoom.Github;

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

    [JsonPropertyName("openai_config")]
    [Description("OpenAI configuration")]
    public OpenAIClientConfiguration? OpenAIConfiguration { get; set; }
}
