using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatRoom.BingSearch.Tests;

public class BingSearchFixture : IDisposable
{
    private readonly Task<int> _start;

    public BingSearchFixture()
    {
        var configurationPath = Path.Combine("configuration", "chatroom-bingsearch.json");
        var configuration = JsonSerializer.Deserialize<BingSearchConfiguration>(File.ReadAllText(configurationPath)) ?? throw new InvalidOperationException("Failed to load configuration file.");
        this.Command = new BingSearchCommand();

        this._start = this.Command.ExecuteAsync(configuration);
        this.Command.DeployAsync().Wait();
    }

    internal BingSearchCommand Command { get; }


    public void Dispose()
    {
        var stopTask = this.Command.StopAsync();

        Task.WhenAll(this._start, stopTask).Wait();
    }
}
