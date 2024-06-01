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

        OpenAIClient? openaiClient = config.OpenAIConfiguration?.ToOpenAIClient();
        string? deployModelName = config.OpenAIConfiguration?.ModelId;

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
            issueHelper = AgentFactory.CreateIssueHelperAgent(openaiClient!, deployModelName!, ghClient, config.IssueHelper.Name, config.IssueHelper.SystemMessage);
        };

        var host = Host.CreateDefaultBuilder()
            .UseChatRoom(roomName: settings.Room ?? "room", port: settings.Port ?? 30000)
            .Build();
        
        var sp = host.Services;

        await host.StartAsync();
        await host.JoinRoomAsync(issueHelper, config.IssueHelper.Description);
        await host.WaitForShutdownAsync();

        return 0;
    }
}
