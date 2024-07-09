using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatRoom.Powershell.Tests;

public class PowershellAgentsFixture : IDisposable
{
    private readonly Task _start;

    public PowershellAgentsFixture()
    {
        var configurationPath = Path.Combine("template", "chatroom.powershell", "chatroom-powershell.json");
        var configuration = JsonSerializer.Deserialize<ChatRoomPowershellConfiguration>(File.ReadAllText(configurationPath)) ?? throw new InvalidOperationException("Failed to load configuration file.");
        this.Command = new PowershellCommand();

        this._start = this.Command.ExecuteAsync(configuration);
        this.Command.DeployAsync().Wait();
    }

    internal PowershellCommand Command { get; }

    public void Dispose()
    {
        var stopTask = this.Command.StopAsync();

        Task.WhenAll(this._start, stopTask).Wait();
    }
}
