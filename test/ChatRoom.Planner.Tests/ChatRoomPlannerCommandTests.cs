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

namespace ChatRoom.Planner.Tests;

public class ChatRoomPlannerCommandTests : IClassFixture<EmptyChatRoomClientFixture>, IClassFixture<ChatRoomPlannerFixture>
{
    private readonly EmptyChatRoomClientFixture _fixture;
    private readonly ChatRoomPlannerFixture _plannerFixture;
    private readonly ChatPlatformClient _client;

    public ChatRoomPlannerCommandTests(EmptyChatRoomClientFixture fixture, ChatRoomPlannerFixture plannerFixture)
    {
        _fixture = fixture;
        _plannerFixture = plannerFixture;
        _client = plannerFixture.Command.ServiceProvider?.GetRequiredService<ChatPlatformClient>() ?? throw new InvalidOperationException("Failed to get ChatPlatformClient.");
    }

    [Fact]
    public async Task ItRegisterAndUnregisterAgentsToRoomAsync()
    {
        var agents = await _client.GetRoomMembers();
        var names = agents.Select(a => a.Name).ToList();
        names.Should().Contain("react-planner");

        var orchestrators = await _client.GetOrchestrators();
        orchestrators.Should().Contain("ReactPlanningOrchestrator");
    }

    [Fact]
    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovalTests")]
    public async Task ItShowHelperTextTestAsync()
    {
        var app = new CommandAppTester();
        app.SetDefaultCommand<ChatRoomPlannerCommand>(ChatRoomPlannerCommand.Description);

        var result = await app.RunAsync("--help");

        Approvals.Verify(result.Output);
    }
}
