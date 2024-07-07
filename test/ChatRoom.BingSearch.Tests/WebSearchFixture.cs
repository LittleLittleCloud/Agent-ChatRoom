using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatRoom.WebSearch.Tests;

public class WebSearchFixture : IDisposable
{
    private readonly Task<int> _start;

    public WebSearchFixture()
    {
        var configurationPath = Path.Combine("template", "chatroom.websearch", "chatroom-websearch.json");
        var configuration = JsonSerializer.Deserialize<WebSearchConfiguration>(File.ReadAllText(configurationPath)) ?? throw new InvalidOperationException("Failed to load configuration file.");
        this.Command = new WebSearchCommand();

        this._start = this.Command.ExecuteAsync(configuration);
        this.Command.DeployAsync().Wait();
    }

    internal WebSearchCommand Command { get; }


    public void Dispose()
    {
        var stopTask = this.Command.StopAsync();

        Task.WhenAll(this._start, stopTask).Wait();
    }
}
