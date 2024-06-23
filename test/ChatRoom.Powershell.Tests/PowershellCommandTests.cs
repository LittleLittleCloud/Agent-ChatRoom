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

namespace ChatRoom.Powershell.Tests;

public class PowershellCommandTests : IClassFixture<DefaultClientFixture>, IClassFixture<PowershellAgentsFixture>
{
    private readonly PowershellAgentsFixture _fixture;
    private readonly DefaultClientFixture _client;
    private readonly ChatPlatformClient _chatPlatformClient;

    public PowershellCommandTests(DefaultClientFixture fixture, PowershellAgentsFixture powershellAgentsFixture)
    {
        _client = fixture;
        _fixture = powershellAgentsFixture;
        _chatPlatformClient = powershellAgentsFixture.Command.ServiceProvider?.GetRequiredService<ChatPlatformClient>() ?? throw new InvalidOperationException("Failed to get ChatPlatformClient.");
    }

    [Fact]
    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovalTests")]
    public async Task ItShowHelperTextTestAsync()
    {
        var app = new CommandAppTester();
        app.SetDefaultCommand<PowershellCommand>(PowershellCommand.Description);

        var result = await app.RunAsync("--help");

        Approvals.Verify(result.Output);
    }

    [Fact]
    public async Task ItAddPowershellAgentsToRoomTestAsync()
    {
        var roomMembers = await _chatPlatformClient.GetRoomMembers();
        roomMembers.Count().Should().Be(3);
        roomMembers.Select(a => a.Name).Should().Contain(["ps-gpt", "ps-runner"]);
    }
}
