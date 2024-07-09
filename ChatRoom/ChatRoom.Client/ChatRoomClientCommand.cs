using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ChatRoom.Github;
using ChatRoom.OpenAI;
using ChatRoom.Powershell;
using ChatRoom.SDK;
using ChatRoom.WebSearch;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ChatRoom.Client;

public class ChatRoomClientCommandSettings : CommandSettings
{
    [Description("Configuration file")]
    [CommandOption("-c|--config <CONFIG>")]
    public string? ConfigFile { get; init; } = null;
}

public class ChatRoomClientCommand : AsyncCommand<ChatRoomClientCommandSettings>
{
    private IHost? _host = null;
    private bool _deployed = false;
    public static string Description { get; } = """
        A Chatroom client.
        
        The client will start a chat room service and attach web ui to it.
        """;

    internal IServiceProvider? ServiceProvider { get => _host?.Services; }

    internal async Task StopAsync()
    {
        if (_host is not null)
        {
            await _host.StopAsync();
        }
    }

    internal async Task DeployAsync()
    {
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

            await Task.Delay(1000);
        }
    }

    public override Task<int> ExecuteAsync(CommandContext _, ChatRoomClientCommandSettings command)
    {
        _deployed = false;
        var config = command.ConfigFile is not null
            ? JsonSerializer.Deserialize<ChatRoomClientConfiguration>(File.ReadAllText(command.ConfigFile))!
            : new ChatRoomClientConfiguration();

        return ExecuteAsync(config);
    }

    internal async Task<int> ExecuteAsync(ChatRoomClientConfiguration config)
    {
        var workspace = config.Workspace;
        if (!Directory.Exists(workspace))
        {
            Directory.CreateDirectory(workspace);
        }

        var dateTimeNow = DateTime.Now;
        var clientLogPath = Path.Combine(workspace, "logs", $"clients-{dateTimeNow:yyyy-MM-dd_HH-mm-ss}.log");
        var debugLogTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] ({SourceContext}) {Message:lj}{NewLine}{Exception}";
        var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();

                var serilogLogger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .WriteTo.File(clientLogPath, outputTemplate: debugLogTemplate)
#if DEBUG
                    .WriteTo.Console(outputTemplate: debugLogTemplate)
#else
                    .WriteTo.Conditional((le) => le.Level >= Serilog.Events.LogEventLevel.Information, lc => lc.Console(outputTemplate: "{Message:lj}{NewLine}{Exception}"))
#endif
                    .CreateLogger();

                loggingBuilder.AddSerilog(serilogLogger);
            })
            .UseChatRoomServer(config);

        _host = hostBuilder.Build();

        var sp = _host.Services;
        var lifetimeManager = sp.GetRequiredService<IHostApplicationLifetime>();
        await _host.StartAsync();
        await AnsiConsole.Status()
            .StartAsync("initializing...", async ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);

                do
                {
                    await Task.Delay(1000);
                }
                while (!lifetimeManager.ApplicationStarted.IsCancellationRequested);
            });


        var logger = sp.GetRequiredService<ILogger<ChatRoomClientCommand>>();

        logger.LogInformation("Client started.");
        logger.LogInformation($"Workspace: {workspace}");
        logger.LogInformation($"client log is saved to: {Path.Combine(workspace, "logs", clientLogPath)}");
        if (config.ServerConfig is ServerConfiguration)
        {
            logger.LogInformation($"web ui is available at: {config.ServerConfig.Urls}");
        }

        var chatRoomClient = sp.GetRequiredService<ChatPlatformClient>();

        if (config.ChatRoomOpenAIConfiguration is ChatRoomOpenAIConfiguration openAIConfig)
        {
            logger.LogInformation("Registering agents from OpenAI configuration.");
            foreach (var agentConfig in openAIConfig.Agents)
            {
                var agentFactory = new OpenAIAgentFactory(agentConfig);
                await chatRoomClient.RegisterAutoGenAgentAsync(agentFactory.CreateAgent(), agentConfig.Description);

                logger.LogInformation($"Agent {agentConfig.Name} registered.");
            }
        }

        if (config.ChatRoomWebSearchConfiguration is ChatRoomWebSearchConfiguration webConfig)
        {
            logger.LogInformation("Registering agents from WebSearch configuration.");
            if (webConfig.BingSearchConfiguration is BingSearchConfiguration bingConfig)
            {
                await chatRoomClient.RegisterAutoGenAgentAsync(WebSearchAgentFactory.CreateBingSearchAgent(bingConfig), bingConfig.Description);
                logger.LogInformation("BingSearch agent registered.");
            }

            if (webConfig.GoogleSearchConfiguration is GoogleSearchConfiguration googleConfig)
            {
                await chatRoomClient.RegisterAutoGenAgentAsync(WebSearchAgentFactory.CreateGoogleSearchAgent(googleConfig), googleConfig.Description);
                logger.LogInformation("GoogleSearch agent registered.");
            }
        }

        if (config.ChatRoomPowershellConfiguration is ChatRoomPowershellConfiguration powershellConfig)
        {
            logger.LogInformation("Registering agents from Powershell configuration.");
            var psGPT = PowershellAgentFactory.CreatePwshDeveloperAgent(powershellConfig.GPT);
            var psRunner = new PowershellRunnerAgent(powershellConfig.Runner.Name, powershellConfig.Runner.LastNMessage);

            await chatRoomClient.RegisterAutoGenAgentAsync(psGPT, powershellConfig.GPT.Description);
            await chatRoomClient.RegisterAutoGenAgentAsync(psRunner, powershellConfig.Runner.Description);
        }

        if (config.ChatRoomGithubConfiguration is ChatRoomGithubConfiguration githubConfig)
        {
            logger.LogInformation("Registering agents from Github configuration.");
            var issueHelperAgent = GithubAgentFactory.CreateIssueHelper(githubConfig);
            await chatRoomClient.RegisterAutoGenAgentAsync(issueHelperAgent, githubConfig.IssueHelper.Description);
        }

        _deployed = true;
        if (config.EnableConsoleApp)
        {
            var consoleApp = sp.GetRequiredService<ChatRoomConsoleApp>();
            await consoleApp.StartAsync(CancellationToken.None);
        }
        else
        {
            await _host.WaitForShutdownAsync();
        }

        await AnsiConsole.Status()
            .StartAsync("shutting down...", async ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                await _host.StopAsync();
                await _host.WaitForShutdownAsync();
            });

        return 0;
    }
}
