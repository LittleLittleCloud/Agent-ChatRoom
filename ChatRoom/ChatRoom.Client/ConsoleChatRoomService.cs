using System.Text.Json;
using ChatRoom.Client.DTO;
using ChatRoom.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace ChatRoom.Client;

public class ConsoleChatRoomService
{
    private readonly IClusterClient _clusterClient;
    private ClientContext _clientContext;
    private readonly IRoomObserver _roomObserverRef;
    private readonly ILogger _logger;
    private readonly string _workspacePath = null!;
    private readonly string _chatRoomContextSchemaPath = null!;
    private readonly ChatRoomClientController _controller;

    public ConsoleChatRoomService(
        ClientContext clientContext,
        ChatRoomClientCommandSettings settings,
        IRoomObserver roomObserver,
        IClusterClient clsterClient,
        ChatRoomClientController controller,
        ILogger<ConsoleChatRoomService> logger)
    {
        _logger = logger;
        _workspacePath = settings.Workspace;
        _chatRoomContextSchemaPath = Path.Combine(_workspacePath, "chat-history.json");
        _roomObserverRef = roomObserver;
        _clusterClient = clsterClient;
        _clientContext = clientContext;
        _controller = controller;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        PrintUsage();
        var room = _clusterClient.GetGrain<IRoomGrain>(_clientContext.CurrentRoom);
        await room.JoinRoom(_clientContext.UserName!, _clientContext.Description!, true, _roomObserverRef);

        // restore previous state
        if (File.Exists(_chatRoomContextSchemaPath))
        {
            AnsiConsole.MarkupLine("[bold red]Restoring workspace from {0}[/]", _workspacePath);
            var schema = JsonSerializer.Deserialize<ChatRoomContextSchemaV0>(File.ReadAllText(_chatRoomContextSchemaPath))!;
            var workspaceConfiguration = new ChatRoomContext(schema);
            foreach (var channel in workspaceConfiguration.Channels)
            {
                var channelName = channel.Key;
                var channelMembers = channel.Value;
                var channelHistory = workspaceConfiguration.ChatHistory.TryGetValue(channelName, out var history) ? history : null;
                await room.CreateChannel(channelName, channelMembers, channelHistory);
                _logger.LogInformation("Restored channel {ChannelName} with {MemberCount} members and {HistoryCount} history items", channelName, channelMembers.Count(), channelHistory?.Count() ?? 0);
            }

            _clientContext.CurrentChannel = workspaceConfiguration.CurrentChannel;
        }

        await JoinChannel(_clientContext.CurrentChannel!);
        await ProcessLoopAsync(_clientContext, cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        //_processTask?.Wait();
        AnsiConsole.MarkupLine("[bold red]Exiting...[/]");
    }

    async Task ProcessLoopAsync(ClientContext context, CancellationToken ct)
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
            } is Task<ClientContext> cxtTask)
            {
                context = await cxtTask;
                continue;
            }

            if (firstTwoCharacters switch
            {
                "/h" => ShowCurrentChannelHistory(context),
                "/m" => ShowChannelMembers(),
                "/s" => SaveContextToWorkspace(context),
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
    }

    public async Task SaveContextToWorkspace(ClientContext context)
    {
        var room = _clusterClient.GetGrain<IRoomGrain>(context.CurrentRoom);
        var channels = await room.GetChannels();
        Dictionary<string, ChatMsg[]> chatHistory = new();
        Dictionary<string, string[]> channelMembers = new();
        foreach (var channel in channels)
        {
            var channelGrain = _clusterClient.GetGrain<IChannelGrain>(channel.Name);
            var history = await channelGrain.ReadHistory(100);
            chatHistory[channel.Name] = history.ToArray();
            var members = await channelGrain.GetMembers();
            channelMembers[channel.Name] = members.Select(m => m.Name).ToArray();
        }

        var workspaceConfiguration = new ChatRoomContext
        {
            Channels = channelMembers,
            ChatHistory = chatHistory,
            CurrentChannel = context.CurrentChannel!,
        };

        var schema = workspaceConfiguration.ToSchema();
        var json = JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });

        _logger.LogInformation("Saving workspace to {WorkspacePath}", _workspacePath);
        AnsiConsole.MarkupLine("[bold red]Saving workspace to {0}[/]", _workspacePath);
        await File.WriteAllTextAsync(_chatRoomContextSchemaPath, json);
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

    public async Task ShowCurrentRoomChannels(ClientContext context)
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

    public async Task ShowCurrentChannelHistory(ClientContext context)
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
            var encodedText = chatMsg.Text.Replace("[", "[[");
            encodedText = encodedText.Replace("]", "]]");
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
        ClientContext context,
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
            await _controller.JoinChannel(new JoinChannelRequest(channelName, true));
            _clientContext.CurrentChannel = channelName;
        });
    }
}
