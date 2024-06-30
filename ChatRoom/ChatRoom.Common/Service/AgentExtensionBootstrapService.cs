using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ChatRoom.SDK;

internal class AgentExtensionBootstrapService : IHostedService
{
    private readonly ChatRoomServerConfiguration _config;
    private readonly ILogger<AgentExtensionBootstrapService> _logger;
    private readonly List<Process> _processes = [];
    public AgentExtensionBootstrapService(ChatRoomServerConfiguration config, ILogger<AgentExtensionBootstrapService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var extensionConfig = _config.AgentExtensions;
        _logger.LogInformation("Loading {ExtensionCount} extensions", extensionConfig.Count);
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

                _logger.LogInformation("Extension {ExtensionName} started with pid {PID}", extension.Name, process.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start extension {ExtensionName}", extension.Name);
                Console.WriteLine("Failed to start extension {0}", extension.Name);
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

                _logger.LogInformation("Extension {ExtensionName} stopped", process.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to stop extension process");
            }
        }
    }

    public Process CreateExtensionProcess(AgentExtensionConfiguration configuration)
    {
        var command = configuration.Command.Trim();
        var fileName = command.Split(' ')[0];
        var arguments = command.Substring(fileName.Length).Trim();
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
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
                _logger.LogInformation("Extension {ExtensionName} (pid: {PID}) output: {Output}", configuration.Name, process.Id, args.Data);
            }
        };

        process.ErrorDataReceived += (sender, args) =>
        {
            if (args.Data is not null)
            {
                _logger.LogError("Extension {ExtensionName} (pid: {PID}) error: {Error}", configuration.Name, process.Id, args.Data);
            }
        };

        return process;
    }
}
