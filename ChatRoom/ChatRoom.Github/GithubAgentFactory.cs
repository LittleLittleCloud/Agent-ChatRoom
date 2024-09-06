using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using Octokit;
using OpenAI;
using OpenAI.Chat;
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
                openaiClient!.GetChatClient(deployModelName!),
                ghClient,
                config.GithubRepoOwner!,
                config.GithubRepoName!,
                config.IssueHelper.Name,
                config.IssueHelper.SystemMessage);
        };

        return issueHelper;
    }

    public static IAgent CreateIssueHelperAgent(
        ChatClient chatClient,
        GitHubClient gitHubClient,
        string repoOwner,
        string repoName,
        string name = "issue-helper",
        string systemMessage = "You are a github issue helper")
    {
        var openaiChatAgent = new OpenAIChatAgent(
            chatClient: chatClient,
            name: name,
            systemMessage: systemMessage)
            .RegisterMessageConnector();

        return new IssueHelper(openaiChatAgent, gitHubClient, repoOwner, repoName);
    }
}
