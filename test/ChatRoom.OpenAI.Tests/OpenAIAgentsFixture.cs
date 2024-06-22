using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatRoom.OpenAI.Tests;

public class OpenAIAgentsFixture : IDisposable
{
    private readonly Task _start;

    public OpenAIAgentsFixture()
    {
        var configurationPath = Path.Combine("test-configuration", "openai-agents.json");
        var configuration = JsonSerializer.Deserialize<Configuration>(File.ReadAllText(configurationPath)) ?? throw new InvalidOperationException("Failed to load configuration file.");
        this.Command = new OpenAICommand();

        this._start = this.Command.ExecuteAsync(configuration);
        var deploy = this.Command.DeployAsync();
        var timeout = Task.Delay(10000);

        Task.WhenAny(deploy, timeout).Wait();

        if (timeout.IsCompleted)
        {
            throw new TimeoutException("Failed to deploy the client in time.");
        }
    }

    public OpenAICommand Command { get; private set; }

    public void Dispose()
    {
        var timeOut = Task.Delay(10000);
        _ = Command.StopAsync();
        Task.WhenAny(this._start, timeOut).Wait();

        if (timeOut.IsCompleted)
        {
            throw new TimeoutException("Failed to stop the client in time.");
        }
    }
}
