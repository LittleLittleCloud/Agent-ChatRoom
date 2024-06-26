﻿using ChatRoom.SDK;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace ChatRoom.SDK;

internal class ChatRoomConsoleApp
{
    private readonly IClusterClient _clusterClient;
    private ConsoleAppContext _clientContext;
    private readonly ILogger _logger;
    private readonly string _workspacePath = null!;
    private readonly string _chatRoomContextSchemaPath = null!;
    private readonly ChatRoomClientController _controller;
    private readonly ChatPlatformClient _chatPlatformClient;
    private readonly ChatRoomServerConfiguration _chatRoomClientConfiguration;

    public ChatRoomConsoleApp(
        ConsoleAppContext clientContext,
        ChatRoomServerConfiguration settings,
        IClusterClient clsterClient,
        ChatRoomClientController controller,
        ChatPlatformClient chatPlatformClient,
        ILogger<ChatRoomConsoleApp> logger)
    {
        _logger = logger;
        _workspacePath = settings.Workspace;
        _chatRoomContextSchemaPath = Path.Combine(_workspacePath, "chat-history.json");
        _clusterClient = clsterClient;
        _clientContext = clientContext;
        _controller = controller;
        _chatPlatformClient = chatPlatformClient;
        _chatRoomClientConfiguration = settings;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        PrintUsage();
        await ProcessLoopAsync(_clientContext, cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        //_processTask?.Wait();
        AnsiConsole.MarkupLine("[bold red]Exiting...[/]");
    }

    async Task ProcessLoopAsync(ConsoleAppContext context, CancellationToken ct)
    {
        string? input = null;
        do
        {
            input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                continue;
            }

            if (input.StartsWith("/exit") &&
                AnsiConsole.Confirm("Do you really want to exit?"))
            {
                if (AnsiConsole.Confirm("Do you want to save the workspace?"))
                {
                    await SaveContextToWorkspace(context);
                }

                break;
            }

            var firstThreeCharacters = input.Length >= 3 ? input[..3] : string.Empty;
            if (firstThreeCharacters switch
            {
                "/lm" => ShowCurrentRoomMembers(),
                "/lc" => ShowCurrentRoomChannels(context),
                "/lo" => ShowCurrentRoomOrchestrators(),
                _ => null
            } is Task queryTask)
            {
                await queryTask;
                continue;
            }

            if (firstThreeCharacters is "/rc")
            {
                var channelName = input.Replace("/rc", "").Trim();
                await _controller.DeleteChannel(new DeleteChannelRequest(channelName));
                continue;
            }

            if (firstThreeCharacters is "/am")
            {
                var memberName = input.Replace("/am", "").Trim();
                await _controller.AddAgentToChannel(new AddAgentToChannelRequest(_clientContext.CurrentChannel!, memberName));
                continue;
            }

            if (firstThreeCharacters is "/rm")
            {
                var memberName = input.Replace("/rm", "").Trim();
                await _controller.RemoveAgentFromChannel(new RemoveAgentFromChannelRequest(_clientContext.CurrentChannel!, memberName));
                continue;
            }

            var firstTwoCharacters = input.Length >= 2 ? input[..2] : string.Empty;

            if (firstTwoCharacters switch
            {
                "/j" => JoinChannel(input.Replace("/j", "").Trim()),
                _ => null
            } is Task<ConsoleAppContext> cxtTask)
            {
                context = await cxtTask;
                continue;
            }

            if (firstTwoCharacters switch
            {
                "/h" => ShowCurrentChannelHistory(context),
                "/m" => ShowChannelMembers(),
                "/s" => SaveContextToWorkspace(context),
                "/l" => LoadCheckpoint(),
                _ => null
            } is Task task)
            {
                await task;
                continue;
            }



            if (context.IsConnectedToChannel)
            {
                await SendMessage(context, input);
            }
        } while (input is not "/exit" && !ct.IsCancellationRequested);
    }

    private async Task ShowCurrentRoomOrchestrators()
    {
        var orchestrators = await _chatPlatformClient.GetOrchestrators();

        if (orchestrators is null)
        {
            AnsiConsole.MarkupLine("[bold red]No orchestrators found[/]");
            return;
        }

        AnsiConsole.Write(new Rule($"Orchestrators for '{_clientContext.CurrentRoom}'")
        {
            Justification = Justify.Center,
            Style = Style.Parse("darkgreen")
        });

        foreach (var orchestrator in orchestrators)
        {
            AnsiConsole.MarkupLine("[bold yellow]{0}[/]", orchestrator);
        }
    }

    private async Task LoadCheckpoint()
    {
        // list all checkpoints
        var checkpointResponses = await _controller.GetRoomCheckpoints();
        var checkpoints = (checkpointResponses.Result as OkObjectResult)?.Value as IEnumerable<string>;

        if (checkpoints is not null && checkpoints.Any())
        {
            AnsiConsole.MarkupLine($"[bold red]Found {checkpoints.Count()} checkpoints[/]");
            foreach (var checkpoint in checkpoints)
            {
                // encode the checkpoint
                Console.WriteLine(checkpoint);
            }

            // load the latest checkpoint
            var latestCheckpoint = checkpoints.First();
            await _controller.LoadCheckpoint(latestCheckpoint);

            Console.WriteLine("Checkpoint loaded.");
        }
    }

    private void PrintUsage()
    {
        AnsiConsole.WriteLine();
        var table = new Table()
        {
            Border = TableBorder.None,
            Expand = true,
        }.HideHeaders();
        table.AddColumn(new TableColumn("One"));

        var header = new FigletText("Agent")
        {
            Color = Color.Aqua
        };
        var header2 = new FigletText("ChatRoom")
        {
            Color = Color.Aqua
        };

        var markup = new Markup(
           "[bold fuchsia]/j[/] [aqua]<channel>[/] to [underline green]join[/] a specific channel\n"
           + "[bold fuchsia]/h[/] to re-read channel [underline green]history[/]\n"
           + "[bold fuchsia]/m[/] to query [underline green]members[/] in the current channel\n"
           + "[bold fuchsia]/s[/] to save the channel information and history to the workspace\n"
           + "[bold fuchsia]/lm[/] to query [underline green]members[/] in the room\n"
           + "[bold fuchsia]/lc[/] to query [underline green]all channels[/] in the room\n"
           + "[bold fuchsia]/lo[/] to query [underline green]orchestrators[/] in the room\n"
           + "[bold fuchsia]/rc[/] [aqua]<channel>[/] to [underline green]remove channel[/] from the room\n"
           + "[bold fuchsia]/am[/] [aqua]<member>[/] to [underline green]add member[/] to the current channel\n"
           + "[bold fuchsia]/rm[/] [aqua]<member>[/] to [underline green]remove member[/] from the current channel\n"
           + "[bold fuchsia]/exit[/] to exit\n"
           + "[bold aqua]<message>[/] to send a [underline green]message[/]\n");
        table.AddColumn(new TableColumn("Two"));

        var rightTable = new Table()
            .HideHeaders()
            .Border(TableBorder.None)
            .AddColumn(new TableColumn("Content"));

        rightTable.AddRow(header)
            .AddRow(header2)
            .AddEmptyRow()
            .AddEmptyRow()
            .AddRow(markup);
        table.AddRow(rightTable);

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[bold green]Client started.[/]");
        AnsiConsole.MarkupLine($"[bold green]Workspace:[/] {_workspacePath}");
        if (_chatRoomClientConfiguration.ServerConfig is ServerConfiguration)
        {
            AnsiConsole.MarkupLine($"[bold green]web ui is available at: {_chatRoomClientConfiguration.ServerConfig.Urls}[/]");
        }
    }

    public async Task SaveContextToWorkspace(ConsoleAppContext context)
    {
        await this._controller.SaveCheckpoint();
    }

    private async Task ShowChannelMembers()
    {
        var memberResponse = await _controller.GetChannelMembers(new GetChannelMembersRequest(_clientContext.CurrentChannel!));
        var members = (memberResponse.Result as OkObjectResult)?.Value as IEnumerable<AgentInfo>;

        if (members is null)
        {
            AnsiConsole.MarkupLine("[bold red]No members found[/]");
            return;
        }


        AnsiConsole.Write(new Rule($"Members for '{_clientContext.CurrentChannel}'")
        {
            Justification = Justify.Center,
            Style = Style.Parse("darkgreen")
        });

        foreach (var member in members)
        {
            AnsiConsole.MarkupLine("[bold yellow]{0}[/]", member);
        }

        AnsiConsole.Write(new Rule()
        {
            Justification = Justify.Center,
            Style = Style.Parse("darkgreen")
        });
    }

    private async Task ShowCurrentRoomMembers()
    {
        var membersRespnose = await _controller.GetRoomMembers();
        var members = (membersRespnose.Result as OkObjectResult)?.Value as IEnumerable<AgentInfo>;

        if (members is null)
        {
            AnsiConsole.MarkupLine("[bold red]No members found[/]");
            return;
        }

        AnsiConsole.Write(new Rule($"Members for '{_clientContext.CurrentRoom}'")
        {
            Justification = Justify.Center,
            Style = Style.Parse("darkgreen")
        });

        foreach (var member in members)
        {
            AnsiConsole.MarkupLine("[bold yellow]{0}[/]", member);
        }

        AnsiConsole.Write(new Rule()
        {
            Justification = Justify.Center,
            Style = Style.Parse("darkgreen")
        });
    }

    public async Task ShowCurrentRoomChannels(ConsoleAppContext context)
    {
        var channelsResponse = await _controller.GetChannels();
        var channels = (channelsResponse.Result as OkObjectResult)?.Value as IEnumerable<ChannelInfo>;

        if (channels is null)
        {
            AnsiConsole.MarkupLine("[bold red]No channels found[/]");
            return;
        }

        AnsiConsole.Write(new Rule($"Channels for '{context.CurrentRoom}'")
        {
            Justification = Justify.Center,
            Style = Style.Parse("darkgreen")
        });

        foreach (var channel in channels)
        {
            AnsiConsole.MarkupLine("[bold yellow]{0}[/]", channel.Name);
        }

        AnsiConsole.Write(new Rule()
        {
            Justification = Justify.Center,
            Style = Style.Parse("darkgreen")
        });
    }

    public async Task ShowCurrentChannelHistory(ConsoleAppContext context)
    {
        var historyResponse = await _controller.GetChannelChatHistory(new GetChannelChatHistoryRequest(context.CurrentChannel!, 1_000));
        var history = (historyResponse.Result as OkObjectResult)?.Value as IEnumerable<ChatMsg>;

        if (history is null)
        {
            AnsiConsole.MarkupLine("[bold red]No history found[/]");
            return;
        }

        AnsiConsole.Write(new Rule($"History for '{context.CurrentChannel}'")
        {
            Justification = Justify.Center,
            Style = Style.Parse("darkgreen")
        });

        foreach (var chatMsg in history)
        {
            var encodedText = chatMsg.GetContent()?.Replace("[", "[[");
            encodedText = encodedText?.Replace("]", "]]");
            encodedText ??= "unsupported format";
            AnsiConsole.MarkupLine("[[[dim]{0}[/]]] [bold yellow]{1}:[/] {2}",
                chatMsg.Created.LocalDateTime, chatMsg.From!, encodedText);
        }

        AnsiConsole.Write(new Rule()
        {
            Justification = Justify.Center,
            Style = Style.Parse("darkgreen")
        });
    }

    public async Task SendMessage(
        ConsoleAppContext context,
        string messageText)
    {
        var message = new ChatMsg(context.UserName!, messageText);
        await _controller.SendTextMessageToChannel(new SendTextMessageToChannelRequest(context.CurrentChannel!, message));
    }

    public async Task JoinChannel(
        string channelName)
    {
        await AnsiConsole.Status().StartAsync("Joining channel...", async ctx =>
        {
            var channels = await _chatPlatformClient.GetChannels();
            if (!channels.Any(x => x.Name == channelName))
            {
                // create the channel
                await _chatPlatformClient.CreateChannel(channelName);
            }

            var members = await _chatPlatformClient.GetChannelMembers(channelName);
            if (!members.Any(x => x.Name == _clientContext.UserName))
            {
                await _chatPlatformClient.AddAgentToChannel(channelName, _clientContext.UserName!);
            }

            _clientContext.CurrentChannel = channelName;
        });
    }
}
