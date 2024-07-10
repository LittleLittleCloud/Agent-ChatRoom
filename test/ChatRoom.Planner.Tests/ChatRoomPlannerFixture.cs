using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatRoom.Planner.Tests;

public class ChatRoomPlannerFixture : IDisposable
{
    private readonly Task _start;

    public ChatRoomPlannerFixture()
    {
        var configurationPath = Path.Combine("template", "chatroom.planner", "chatroom-planner.json");
        var configuration = JsonSerializer.Deserialize<ChatRoomPlannerConfiguration>(File.ReadAllText(configurationPath)) ?? throw new InvalidOperationException("Failed to load configuration file.");
        this.Command = new ChatRoomPlannerCommand();
        this._start = this.Command.ExecuteAsync(configuration);
        this.Command.DeployAsync().Wait();
    }

    internal ChatRoomPlannerCommand Command { get; }

    public void Dispose()
    {
        var stopTask = this.Command.StopAsync();

        Task.WhenAll(this._start, stopTask).Wait();
    }
}
