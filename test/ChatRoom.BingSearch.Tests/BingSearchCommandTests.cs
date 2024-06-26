﻿using System;
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

namespace ChatRoom.BingSearch.Tests;

public class BingSearchCommandTests : IClassFixture<DefaultClientFixture>, IClassFixture<BingSearchFixture>
{
    private readonly DefaultClientFixture _fixture;
    private readonly BingSearchFixture _bingSearchFixture;
    private readonly ChatPlatformClient _client;

    public BingSearchCommandTests(DefaultClientFixture fixture, BingSearchFixture bingSearchFixture)
    {
        _fixture = fixture;
        _bingSearchFixture = bingSearchFixture;
        _client = bingSearchFixture.Command.ServiceProvider?.GetRequiredService<ChatPlatformClient>() ?? throw new InvalidOperationException("Failed to get ChatPlatformClient.");
    }

    [Fact]
    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovalTests")]
    public async Task ItShowHelperTextTestAsync()
    {
        var app = new CommandAppTester();
        app.SetDefaultCommand<BingSearchCommand>(BingSearchCommand.Description);

        var result = await app.RunAsync("--help");

        Approvals.Verify(result.Output);
    }

    [Fact]
    public async Task ItRegisterAndUnregisterAgentsToRoomAsync()
    {
        var agents = await _client.GetRoomMembers();
        var names = agents.Select(a => a.Name).ToList();
        names.Should().Contain("bing-search");
    }
}
