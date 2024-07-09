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
using Xunit.Abstractions;
using ChatRoom.SDK;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;

namespace ChatRoom.Client.Tests;

public class ChatRoomClientCommandTests : IClassFixture<AllInOneChatRoomClientFixture>
{
    private readonly AllInOneChatRoomClientFixture _fixture;
    private readonly ChatPlatformClient _client;

    public ChatRoomClientCommandTests(AllInOneChatRoomClientFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _client = fixture.Command.ServiceProvider?.GetRequiredService<ChatPlatformClient>() ?? throw new InvalidOperationException("Failed to get ChatPlatformClient.");
    }

    [Fact]
    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovalTests")]
    public async Task ItShowHelperTextTestAsync()
    {
        var app = new CommandAppTester();
        app.SetDefaultCommand<ChatRoomClientCommand>(ChatRoomClientCommand.Description);

        var result = await app.RunAsync("--help");

        Approvals.Verify(result.Output);
    }

    [Fact]
    public async Task ItRegisterAllAgentsToTheRoom()
    {
        var agents = await _client.GetRoomMembers();
        var names = agents.Select(a => a.Name).ToList();


        //names.Should().BeEquivalentTo("User", "gpt3.5", "gpt4", "llama3", "bing-search", "google-search", "ps-gpt", "ps-runner", "issue-helper");
        names.Should().Contain("User");
        names.Should().Contain("gpt3.5");
        names.Should().Contain("gpt4");
        names.Should().Contain("llama3");
        names.Should().Contain("bing-search");
        names.Should().Contain("google-search");
        names.Should().Contain("ps-gpt");
        names.Should().Contain("ps-runner");
        names.Should().Contain("issue-helper");

        var orchestrators = await _client.GetOrchestrators();
        //orchestrators.Should().BeEquivalentTo("RoundRobin", "HumanToAgent", "DynamicGroupChat");

        orchestrators.Should().Contain("RoundRobin");
        orchestrators.Should().Contain("HumanToAgent");
        orchestrators.Should().Contain("DynamicGroupChat");
    }

    [Fact]
    public async Task ItCreateAndRemoveChannelTestAsync()
    {
        var channels = await _client.GetChannels();
        channels.Count().Should().Be(0);

        await _client.CreateChannel(nameof(ItCreateAndRemoveChannelTestAsync));
        channels = await _client.GetChannels();
        channels.Count().Should().Be(1);

        // create 5 channels
        for (int i = 0; i < 5; i++)
        {
            await _client.CreateChannel(i.ToString());
        }

        channels = await _client.GetChannels();
        channels.Count().Should().Be(6);

        // remove all channels
        foreach (var channel in channels)
        {
            await _client.DeleteChannel(channel.Name);
        }

        channels = await _client.GetChannels();
        channels.Count().Should().Be(0);
    }
}
