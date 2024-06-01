using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AutoGen.Core;
using Azure.AI.OpenAI;
using ChatRoom.SDK;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel.Agents;
using Octokit;
using Spectre.Console.Cli;

namespace ChatRoom.Github;

internal class GithubCommand : AsyncCommand<ChatRoomAgentClientCommandSettings>
{
    public static string Description { get; } = """
        Github agents for ChatRoom

        The following agents are available:
        - issue-helper: A github issue helper agent
        """;

    public override async Task<int> ExecuteAsync(CommandContext context, ChatRoomAgentClientCommandSettings settings)
    {
        var config = settings.ConfigFile is not null
            ? JsonSerializer.Deserialize<GithubConfiguration>(File.ReadAllText(settings.ConfigFile))!
            : new GithubConfiguration();

        OpenAIClient? openaiClient = null;
        string? deployModelName = null;
        IAgent? issueHelper = null;
        if (config.LLMType == LLMType.AOAI)
        {
            if (config.AzureOpenAiKey is string
            && config.AzureOpenAiEndpoint is string
            && config.AzureDeploymentName is string)
            {
                openaiClient = new OpenAIClient(new Uri(config.AzureOpenAiEndpoint), new Azure.AzureKeyCredential(config.AzureOpenAiKey));
                deployModelName = config.AzureDeploymentName;
            }
            else
            {
                var defaultReply = "Azure OpenAI endpoint, key, or deployment name not found. Please provide Azure OpenAI endpoint, key, and deployment name in the configuration file or via env:AZURE_OPENAI_ENDPOINT, env:AZURE_OPENAI_API_KEY, env:AZURE_OPENAI_DEPLOY_NAME";
                issueHelper = new DefaultReplyAgent(config.IssueHelper.Name, defaultReply);
            }
        }
        else
        {
            if (config.OpenAiApiKey is string && config.OpenAiModelId is string)
            {
                openaiClient = new OpenAIClient(config.OpenAiApiKey);
                deployModelName = config.OpenAiModelId;
            }
            else
            {
                var defaultReply = "OpenAI API key or model id not found. Please provide OpenAI API key and model id in the configuration file or via env:OPENAI_API_KEY, env:OPENAI_MODEL_ID";
                issueHelper = new DefaultReplyAgent(config.IssueHelper.Name, defaultReply);
            }
        }

        var ghClient = new GitHubClient(new ProductHeaderValue("ChatRoom"));
        if (config.GithubToken is string)
        {
            ghClient.Credentials = new Credentials(config.GithubToken);
        }

        if (issueHelper is null)
        {
            issueHelper = AgentFactory.CreateIssueHelperAgent(openaiClient!, deployModelName!, ghClient, config.IssueHelper.Name, config.IssueHelper.SystemMessage);
        };

        var host = Host.CreateDefaultBuilder()
            .AddAgentAsync(issueHelper, config.IssueHelper.Description)
            .UseChatRoom(roomName: settings.Room ?? "room", port: settings.Port ?? 30000)
            .Build();

        await host.StartAsync();
        await host.WaitForAgentsJoinRoomAsync();
        await host.WaitForShutdownAsync();

        return 0;
    }
}
