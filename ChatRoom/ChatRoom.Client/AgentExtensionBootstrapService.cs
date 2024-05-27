using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ChatRoom.Client;

public class AgentExtensionBootstrapService : IHostedService
{
    private readonly ChatRoomClientConfiguration _config;
    private readonly ILogger<AgentExtensionBootstrapService> _logger;
    private readonly List<Process> _processes = [];
    public AgentExtensionBootstrapService(ChatRoomClientConfiguration config, ILogger<AgentExtensionBootstrapService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var extensionConfig = _config.AgentExtensions;
        foreach (var extension in extensionConfig)
        {
            _logger.LogInformation("Loading extension {ExtensionName}", extension.Name);
            var process = CreateExtensionProcess(extension);
            _processes.Add(process);

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start extension {ExtensionName}", extension.Name);
            }
        }
    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // gracefully stop all processes
        foreach (var process in _processes)
        {
            try
            {
                process.Kill();
                await process.WaitForExitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to stop extension process");
            }
        }
    }

    public Process CreateExtensionProcess(AgentExtensionConfiguration configuration)
    {
        var command = configuration.Command;

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = command,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        process.OutputDataReceived += (sender, args) =>
        {
            if (args.Data is not null)
            {
                _logger.LogInformation("Extension {ExtensionName} output: {Output}", configuration.Name, args.Data);
            }
        };

        process.ErrorDataReceived += (sender, args) =>
        {
            if (args.Data is not null)
            {
                _logger.LogError("Extension {ExtensionName} error: {Error}", configuration.Name, args.Data);
            }
        };

        return process;
    }
}
