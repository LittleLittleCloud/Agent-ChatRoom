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

        var timeout = Task.Delay(10000);
        var deployTask = this.Command.DeployAsync();

        Task.WhenAny(deployTask, timeout).Wait();
        if (timeout.IsCompleted)
        {
            throw new TimeoutException("Failed to deploy the client in time.");
        }
    }

    public ChatRoomClientCommand Command { get; private set; }

    public void Dispose()
    {
        _ = Command.StopAsync();
        var timeout = Task.Delay(10000);
        Task.WhenAny(_start, timeout).Wait();
        if (timeout.IsCompleted)
        {
            throw new TimeoutException("Failed to stop the client in time.");
        }
    }
}
