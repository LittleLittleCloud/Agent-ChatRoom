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

namespace ChatRoom.Client.Tests;

public class ChatRoomClientCommandTests : IClassFixture<DefaultClientFixture>
{
    private readonly DefaultClientFixture _fixture;
    private readonly ChatPlatformClient _client;

    public ChatRoomClientCommandTests(DefaultClientFixture fixture, ITestOutputHelper output)
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
