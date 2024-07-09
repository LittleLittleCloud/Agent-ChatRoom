using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AutoGen.Core;
using Azure.AI.OpenAI;
using ChatRoom.SDK;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel.Agents;
using Octokit;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ChatRoom.Github;

internal class GithubCommand : AsyncCommand<ChatRoomAgentClientCommandSettings>
{
    private IHost? _host = null;
    private bool _deployed = false;

    public static string Description { get; } = """
        Github agents for ChatRoom

        The following agents are available:
        - issue-helper: A github issue helper agent.
        """;

    public override async Task<int> ExecuteAsync(CommandContext context, ChatRoomAgentClientCommandSettings settings)
    {
        var config = settings.ConfigFile is not null
            ? JsonSerializer.Deserialize<ChatRoomGithubConfiguration>(File.ReadAllText(settings.ConfigFile))!
            : new ChatRoomGithubConfiguration();

        return await ExecuteAsync(config);
    }

    internal async Task<int> ExecuteAsync(ChatRoomGithubConfiguration config)
    {
        if (config.GithubRepoOwner is null || config.GithubRepoName is null)
        {
            AnsiConsole.MarkupLine("[red]Github repo owner and name are required.[/]");
            return 1;
        }

        _host = Host.CreateDefaultBuilder()
            .UseChatRoomClient(roomName: config.RoomConfig.Room ?? "room", port: config.RoomConfig.Port)
            .Build();
        await _host.StartAsync();

        var sp = _host.Services;
        var chatPlatformClient = sp.GetRequiredService<ChatPlatformClient>() ?? throw new InvalidOperationException("ChatPlatformClient is not registered");

        var issueHelper = GithubAgentFactory.CreateIssueHelper(config);
        await chatPlatformClient.RegisterAutoGenAgentAsync(issueHelper, config.IssueHelper.Description);

        _deployed = true;

        await _host.WaitForShutdownAsync();

        return 0;
    }

    internal IServiceProvider? ServiceProvider => _host?.Services;

    internal async Task StopAsync(int maxWaitingTimeInSeconds = 10)
    {
        if (_host is not null)
        {
            var timeout = Task.Delay(TimeSpan.FromSeconds(maxWaitingTimeInSeconds));
            var stopHostTask = _host.StopAsync();

            await Task.WhenAny(timeout, stopHostTask);

            if (timeout.IsCompleted)
            {
                throw new TimeoutException("Stop host timeout");
            }
        }
    }

    internal async Task DeployAsync(int maxWaitingTimeInSeconds = 20)
    {
        var timeOut = Task.Delay(TimeSpan.FromSeconds(maxWaitingTimeInSeconds));
        while (true)
        {
            if (_host is null)
            {
                await Task.Delay(1000);
                continue;
            }

            if (_deployed)
            {
                break;
            }

            if (timeOut.IsCompleted)
            {
                throw new TimeoutException("Deploy timeout");
            }

            await Task.Delay(1000);
        }
    }

}
