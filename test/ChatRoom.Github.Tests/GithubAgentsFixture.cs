using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatRoom.Github.Tests;

public class GithubAgentsFixture : IDisposable
{
    private readonly Task _start;

    public GithubAgentsFixture()
    {
        var configurationPath = Path.Combine("template", "chatroom.github", "chatroom-github.json");
        var configuration = JsonSerializer.Deserialize<GithubConfiguration>(File.ReadAllText(configurationPath)) ?? throw new InvalidOperationException("Failed to load configuration file.");
        this.Command = new GithubCommand();

        this._start = this.Command.ExecuteAsync(configuration);
        this.Command.DeployAsync().Wait();
    }

    internal GithubCommand Command { get; }

    public void Dispose()
    {
        var stopTask = this.Command.StopAsync();

        Task.WhenAll(this._start, stopTask).Wait();
    }
}
