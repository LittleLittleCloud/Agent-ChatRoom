using System.ComponentModel;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;

namespace ChatRoom.SDK;
internal class ChatRoomAgentClientCommandSettings : CommandSettings
{
    [Description("Configuration file.")]
    [CommandOption("-c|--config <CONFIG>")]
    public string? ConfigFile { get; init; }
}

internal abstract class ChatRoomAgentCommand : AsyncCommand<ChatRoomAgentClientCommandSettings>
{
    protected private IHost? _host = null;
    protected private bool _deployed = false;


    internal IServiceProvider? ServiceProvider => _host?.Services;

    internal async Task StopAsync(int maxWaitingTimeInSeconds = 10)
    {
        if (_host is not null)
        {
            var timeout = Task.Delay(TimeSpan.FromSeconds(maxWaitingTimeInSeconds));
            var stopHostTask = _host.StopAsync();

            await Task.WhenAny(timeout, stopHostTask);

            if (timeout.IsCompleted)
            {
                throw new TimeoutException("Stop host timeout");
            }
        }
    }

    internal async Task DeployAsync(int maxWaitingTimeInSeconds = 20)
    {
        var timeOut = Task.Delay(TimeSpan.FromSeconds(maxWaitingTimeInSeconds));
        while (true)
        {
            if (_host is null)
            {
                await Task.Delay(1000);
                continue;
            }

            if (_deployed)
            {
                break;
            }

            if (timeOut.IsCompleted)
            {
                throw new TimeoutException("Deploy timeout");
            }

            await Task.Delay(1000);
        }
    }
}
