using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ChatRoom.SDK;
using Spectre.Console.Testing;
using Xunit.Abstractions;

namespace ChatRoom.Client.Tests;

public abstract class ChatRoomClientFixture : IDisposable
{
    private readonly Task _start;
    private readonly ChatRoomClientConfiguration _configuration;
    public ChatRoomClientFixture(string templateName)
    {
        // called once before every test
        var configurationPath = Path.Combine("template", "chatroom", $"{templateName}.json"); // "template/chatroom/chatroom.json
        this._configuration = JsonSerializer.Deserialize<ChatRoomClientConfiguration>(File.ReadAllText(configurationPath)) ?? throw new InvalidOperationException("Failed to load configuration file.");
        this.Command = new ChatRoomClientCommand();
        this._start = this.Command.ExecuteAsync(this._configuration);

        var timeout = Task.Delay(10000);
        var deployTask = this.Command.DeployAsync();

        Task.WhenAny(deployTask, timeout).Wait();
        if (timeout.IsCompleted)
        {
            throw new TimeoutException("Failed to deploy the client in time.");
        }
    }

    internal ChatRoomClientConfiguration Configuration => _configuration;

    internal ChatRoomClientCommand Command { get; private set; }

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

public class EmptyChatRoomClientFixture : ChatRoomClientFixture
{
    public EmptyChatRoomClientFixture() : base("chatroom_empty")
    {
        // called once before every test
    }
}

public class AllInOneChatRoomClientFixture : ChatRoomClientFixture
{
    public AllInOneChatRoomClientFixture() : base("chatroom_all_in_one")
    {
        // called once before every test
    }
}
