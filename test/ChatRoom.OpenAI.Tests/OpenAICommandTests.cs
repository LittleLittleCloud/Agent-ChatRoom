using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using ChatRoom.Client.Tests;
using ChatRoom.SDK;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using Spectre.Console.Testing;
using Xunit;

namespace ChatRoom.OpenAI.Tests;

public class OpenAICommandTests : IClassFixture<DefaultClientFixture>, IClassFixture<OpenAIAgentsFixture>
{
    private readonly DefaultClientFixture _fixture;
    private readonly OpenAIAgentsFixture _openAIAgentsFixture;
    private readonly ChatPlatformClient _client;

    public OpenAICommandTests(DefaultClientFixture fixture, OpenAIAgentsFixture openAIAgentsFixture)
    {
        _fixture = fixture;
        _openAIAgentsFixture = openAIAgentsFixture;
        _client = openAIAgentsFixture.Command.ServiceProvider?.GetRequiredService<ChatPlatformClient>() ?? throw new InvalidOperationException("Failed to get ChatPlatformClient.");
    }

    [Fact]
    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovalTests")]
    public async Task ItShowHelperTextTestAsync()
    {
        var app = new CommandAppTester();
        app.SetDefaultCommand<OpenAICommand>(OpenAICommand.Description);
        var result = await app.RunAsync("--help");

        Approvals.Verify(result.Output);
    }

    [Fact]
    public async Task ItRegisterAndUnregisterAgentsToRoomAsync()
    {
        var agents = await _client.GetRoomMembers();
        var names = agents.Select(a => a.Name).ToList();
        names.Should().BeEquivalentTo(["User", "gpt35", "gpt4", "llama3"]);
    }
}
