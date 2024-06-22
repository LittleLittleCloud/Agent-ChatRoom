using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Spectre.Console.Testing;
using Xunit.Abstractions;

namespace ChatRoom.Client.Tests;

public class DefaultClientFixture : IDisposable
{
    private readonly Task _start;
    public DefaultClientFixture()
    {
        // called once before every test
        var configurationPath = Path.Combine("test-configuration", "chatroom-client.json");
        var configuration = JsonSerializer.Deserialize<ChatRoomClientConfiguration>(File.ReadAllText(configurationPath)) ?? throw new InvalidOperationException("Failed to load configuration file.");
        this.Command = new ChatRoomClientCommand();
        this._start = this.Command.ExecuteAsync(configuration);
        this.Command.DeployAsync().Wait();
    }

    public ChatRoomClientCommand Command { get; private set; }

    public void Dispose()
    {
        _ = Command.StopAsync();
        this._start.Wait();
    }
}
