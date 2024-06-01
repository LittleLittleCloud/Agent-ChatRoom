using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using Azure.AI.OpenAI;
using ChatRoom.Tests;
using FluentAssertions;
using Octokit;

namespace ChatRoom.Github.Tests;

public class IssueHelperAgentTests
{
    [EnvVariableFact("AZURE_OPENAI_API_KEY", "AZURE_OPENAI_DEPLOY_NAME", "AZURE_OPENAI_ENDPOINT")]
    public async Task ItGetChatRoomIssueTestAsync()
    {
        var aoaiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
        var aoaiDeployName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOY_NAME");
        var aoaiEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");

        var openaiClient = new OpenAIClient(new Uri(aoaiEndpoint!), new Azure.AzureKeyCredential(aoaiKey!));
        
        var ghClient = new GitHubClient(new ProductHeaderValue("chatroom-github-test"));
        var issueHelperAgent = AgentFactory.CreateIssueHelperAgent(openaiClient, aoaiDeployName!, ghClient);

        var reply = await issueHelperAgent.SendAsync("What's the first issue in the https://github.com/LittleLittleCloud/Agent-ChatRoom repository?");

        reply.Should().BeOfType<ToolCallAggregateMessage>();
        var aggregateMessage = reply as ToolCallAggregateMessage;
        aggregateMessage!.Message1.Should().BeOfType<ToolCallMessage>();
        var toolCallMessage = aggregateMessage.Message1;
        toolCallMessage!.ToolCalls[0].FunctionName.Should().Be("GetIssueAsync");
    }

    [EnvVariableFact("AZURE_OPENAI_API_KEY", "AZURE_OPENAI_DEPLOY_NAME", "AZURE_OPENAI_ENDPOINT")]
    public async Task ItSearchChatRoomIssueTestAsync()
    {
        var aoaiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
        var aoaiDeployName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOY_NAME");
        var aoaiEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");

        var openaiClient = new OpenAIClient(new Uri(aoaiEndpoint!), new Azure.AzureKeyCredential(aoaiKey!));

        var ghClient = new GitHubClient(new ProductHeaderValue("chatroom-github-test"));
        var issueHelperAgent = AgentFactory.CreateIssueHelperAgent(openaiClient, aoaiDeployName!, ghClient);

        var reply = await issueHelperAgent.SendAsync("Search issues related to Add documents in https://github.com/LittleLittleCloud/Agent-ChatRoom repository. Return the first issue.");

        reply.Should().BeOfType<ToolCallAggregateMessage>();
        var aggregateMessage = reply as ToolCallAggregateMessage;
        aggregateMessage!.Message1.Should().BeOfType<ToolCallMessage>();
        var toolCallMessage = aggregateMessage.Message1;
        toolCallMessage!.ToolCalls[0].FunctionName.Should().Be("SearchIssuesAsync");
    }

    [EnvVariableFact("AZURE_OPENAI_API_KEY", "AZURE_OPENAI_DEPLOY_NAME", "AZURE_OPENAI_ENDPOINT")]
    public async Task ItGetChatRoomIssueCommentsTestAsync()
    {
        var aoaiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
        var aoaiDeployName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOY_NAME");
        var aoaiEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");

        var openaiClient = new OpenAIClient(new Uri(aoaiEndpoint!), new Azure.AzureKeyCredential(aoaiKey!));

        var ghClient = new GitHubClient(new ProductHeaderValue("chatroom-github-test"));
        var issueHelperAgent = AgentFactory.CreateIssueHelperAgent(openaiClient, aoaiDeployName!, ghClient);

        var reply = await issueHelperAgent.SendAsync("What's the comments of the 9th issue in the https://github.com/LittleLittleCloud/Agent-ChatRoom repository?");

        reply.Should().BeOfType<ToolCallAggregateMessage>();
        var aggregateMessage = reply as ToolCallAggregateMessage;
        aggregateMessage!.Message1.Should().BeOfType<ToolCallMessage>();
        var toolCallMessage = aggregateMessage.Message1;
        toolCallMessage!.ToolCalls[0].FunctionName.Should().Be("GetIssueCommentsAsync");
    }
}
