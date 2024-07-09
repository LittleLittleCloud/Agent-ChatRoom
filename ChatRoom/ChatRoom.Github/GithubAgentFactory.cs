using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using Azure.AI.OpenAI;
using Octokit;
using Spectre.Console;

namespace ChatRoom.Github;

internal static class GithubAgentFactory
{
    public static IAgent CreateIssueHelper(ChatRoomGithubConfiguration config)
    {
        if (config.GithubRepoOwner is null || config.GithubRepoName is null)
        {
            AnsiConsole.MarkupLine("[red]Github repo owner and name are required.[/]");

            return new DefaultReplyAgent(config.IssueHelper.Name, "Github repo owner and name are required.");
        }

        // create issue helper
        OpenAIClient? openaiClient = config.IssueHelper.OpenAIConfiguration?.ToOpenAIClient();
        string? deployModelName = config.IssueHelper.OpenAIConfiguration?.ModelId;

        IAgent? issueHelper = null;

        if (openaiClient is null || deployModelName is null)
        {
            var defaultReply = $"{config.IssueHelper.Name} is not configured properly. Please check the configuration file.";
            issueHelper = new DefaultReplyAgent(config.IssueHelper.Name, defaultReply);
        }

        var ghClient = new GitHubClient(new ProductHeaderValue("ChatRoom"));
        if (config.GithubToken is string)
        {
            ghClient.Credentials = new Credentials(config.GithubToken);
        }

        if (issueHelper is null)
        {
            issueHelper = GithubAgentFactory.CreateIssueHelperAgent(
                openaiClient!,
                deployModelName!,
                ghClient,
                config.GithubRepoOwner!,
                config.GithubRepoName!,
                config.IssueHelper.Name,
                config.IssueHelper.SystemMessage);
        };

        return issueHelper;
    }

    public static IAgent CreateIssueHelperAgent(
        OpenAIClient openaiClient,
        string deployModelName,
        GitHubClient gitHubClient,
        string repoOwner,
        string repoName,
        string name = "issue-helper",
        string systemMessage = "You are a github issue helper")
    {
        var openaiChatAgent = new OpenAIChatAgent(
            openAIClient: openaiClient,
            name: name,
            modelName: deployModelName!,
            systemMessage: systemMessage)
            .RegisterMessageConnector();

        return new IssueHelper(openaiChatAgent, gitHubClient, repoOwner, repoName);
    }
}
