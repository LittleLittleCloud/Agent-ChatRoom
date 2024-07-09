using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using Azure.AI.OpenAI;
using Octokit;

namespace ChatRoom.Github;

internal static class AgentFactory
{
    public static IssueHelper CreateIssueHelperAgent(
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
