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
        this.Command.DeployAsync().Wait();
    }

    public OpenAICommand Command { get; private set; }

    public void Dispose()
    {
        _ = Command.StopAsync();
        this._start.Wait();
    }
}
