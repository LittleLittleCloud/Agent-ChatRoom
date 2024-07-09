using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using ApprovalTests;
using Spectre.Console.Testing;
using Xunit;
using ChatRoom.Client.Tests;
using ChatRoom.SDK;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;

namespace ChatRoom.Github.Tests;

public class GithubCommandTests : IClassFixture<EmptyChatRoomClientFixture>, IClassFixture<GithubAgentsFixture>
{
    private readonly EmptyChatRoomClientFixture _fixture;
    private readonly GithubAgentsFixture _githubFixture;
    private readonly ChatPlatformClient _client;

    public GithubCommandTests(EmptyChatRoomClientFixture fixture, GithubAgentsFixture githubFixture)
    {
        _fixture = fixture;
        _githubFixture = githubFixture;
        _client = githubFixture.Command.ServiceProvider?.GetRequiredService<ChatPlatformClient>() ?? throw new InvalidOperationException("Failed to get ChatPlatformClient.");
    }

    [Fact]
    public async Task ItRegisterAndUnregisterAgentsToRoomAsync()
    {
        var agents = await _client.GetRoomMembers();
        var names = agents.Select(a => a.Name).ToList();
        names.Should().Contain("issue-helper");
    }

    [Fact]
    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovalTests")]
    public async Task ItShowHelperTextTestAsync()
    {
        var app = new CommandAppTester();
        app.SetDefaultCommand<GithubCommand>(GithubCommand.Description);

        var result = await app.RunAsync("--help");

        Approvals.Verify(result.Output);
    }
}
