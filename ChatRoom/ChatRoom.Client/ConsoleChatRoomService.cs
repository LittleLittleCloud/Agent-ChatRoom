using System.Text.Json;
using ChatRoom.Common;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace ChatRoom.Client;

public class ConsoleChatRoomService
{
    private readonly IClusterClient _clusterClient;
    private ClientContext _clientContext;
    private readonly ConsoleRoomObserver _roomObserver;
    private readonly IRoomObserver _roomObserverRef;
    private readonly ILogger _logger;
    private readonly string _workspacePath = null!;
    private readonly string _workspaceChatHistoryPath = null!;

    public ConsoleChatRoomService(
        ChatRoomClientConfiguration configuration,
        IClusterClient clsterClient,
        ILogger<ConsoleChatRoomService> logger)
    {
        _logger = logger;
        _workspacePath = configuration.Workspace;
        _workspaceChatHistoryPath = Path.Combine(_workspacePath, "chat-history.json");
        _roomObserver = new ConsoleRoomObserver();
        _roomObserverRef = clsterClient.CreateObjectReference<IRoomObserver>(_roomObserver);
        _clusterClient = clsterClient;
        _clientContext = new ClientContext(_clusterClient, UserName: configuration.YourName, CurrentChannel: "General", CurrentRoom: configuration.RoomConfig.Room);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        PrintUsage();
        var room = _clientContext.ChannelClient.GetGrain<IRoomGrain>(_clientContext.CurrentRoom);
        await room.JoinRoom(_clientContext.UserName!, _clientContext.Description!, true, _roomObserverRef);

        // restore previous state
        if (File.Exists(_workspaceChatHistoryPath))
        {
            AnsiConsole.MarkupLine("[bold red]Restoring workspace from {0}[/]", _workspacePath);
            var workspaceConfiguration = JsonSerializer.Deserialize<ChatRoomContext>(File.ReadAllText(_workspaceChatHistoryPath))!;
            foreach (var channel in workspaceConfiguration.Channels)
            {
                var channelName = channel.Key;
                var channelMembers = channel.Value;
                var channelHistory = workspaceConfiguration.ChatHistory.TryGetValue(channelName, out var history) ? history : null;
                await room.CreateChannel(channelName, channelMembers, channelHistory);
                _logger.LogInformation("Restored channel {ChannelName} with {MemberCount} members and {HistoryCount} history items", channelName, channelMembers.Count(), channelHistory?.Count() ?? 0);
            }

            _clientContext = _clientContext with { CurrentChannel = workspaceConfiguration.CurrentChannel };
        }

        await JoinChannel(_clientContext, _clientContext.CurrentChannel!);
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
                "/lm" => ShowCurrentRoomMembers(context),
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
                var room = context.ChannelClient.GetGrain<IRoomGrain>(context.CurrentRoom);
                var channels = await room.GetChannels();
                if (channels.Any(c => c.Name == channelName) is false)
                {
                    AnsiConsole.MarkupLine("[bold red]Channel '{0}' does not exist[/]", channelName);
                    continue;
                }
                else
                {
                    await room.DeleteChannel(channelName);
                    continue;
                }
            }

            if (firstThreeCharacters is "/am")
            {
                var part = input.Replace("/am", "").Trim();
                var memberName = part;
                var room = context.ChannelClient.GetGrain<IRoomGrain>(context.CurrentRoom);
                var channels = await room.GetChannels();
                var members = await room.GetMembers();
                if (channels.Any(c => c.Name == context.CurrentChannel) is false)
                {
                    AnsiConsole.MarkupLine("[bold red]Channel '{0}' does not exist[/]", context.CurrentChannel!);
                    continue;
                }
                else if (members.Any(m => m.Name == memberName) is false)
                {
                    AnsiConsole.MarkupLine("[bold red]Member '{0}' does not exist[/]", memberName);
                    continue;
                }
                else
                {
                    var channelInfo = channels.First(c => c.Name == context.CurrentChannel);
                    var memberInfo = members.First(m => m.Name == memberName);
                    var channel = context.ChannelClient.GetGrain<IChannelGrain>(context.CurrentChannel);
                    var channelMembers = await channel.GetMembers();
                    if (channelMembers.Any(m => m.Name == memberName) is true)
                    {
                        AnsiConsole.MarkupLine("[bold red]Member '{0}' already exists in the channel[/]", memberName);
                        continue;
                    }

                    await room.AddAgentToChannel(channelInfo, memberName);
                    continue;
                }
            }

            if (firstThreeCharacters is "/rm")
            {
                var memberName = input.Replace("/rm", "").Trim();
                var room = context.ChannelClient.GetGrain<IRoomGrain>(context.CurrentRoom);
                var channels = await room.GetChannels();
                var members = await room.GetMembers();
                if (channels.Any(c => c.Name == context.CurrentChannel) is false)
                {
                    AnsiConsole.MarkupLine("[bold red]Channel '{0}' does not exist[/]", context.CurrentChannel!);
                    continue;
                }
                else if (members.Any(m => m.Name == memberName) is false)
                {
                    AnsiConsole.MarkupLine("[bold red]Member '{0}' does not exist[/]", memberName);
                    continue;
                }
                else
                {
                    var channelInfo = channels.First(c => c.Name == context.CurrentChannel);
                    var memberInfo = members.First(m => m.Name == memberName);
                    await room.RemoveAgentFromChannel(channelInfo, memberName);

                    continue;
                }
            }

            var firstTwoCharacters = input.Length >= 2 ? input[..2] : string.Empty;

            if (firstTwoCharacters switch
            {
                "/j" => JoinChannel(context, input.Replace("/j", "").Trim()),
                "/l" => LeaveChannel(context),
                _ => null
            } is Task<ClientContext> cxtTask)
            {
                context = await cxtTask;
                continue;
            }

            if (firstTwoCharacters switch
            {
                "/h" => ShowCurrentChannelHistory(context),
                "/m" => ShowChannelMembers(context),
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
           + "[bold fuchsia]/l[/] to [underline green]leave[/] the current channel\n"
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

    private async Task SaveContextToWorkspace(ClientContext context)
    {
        var room = context.ChannelClient.GetGrain<IRoomGrain>(context.CurrentRoom);
        var channels = await room.GetChannels();
        Dictionary<string, ChatMsg[]> chatHistory = new();
        Dictionary<string, string[]> channelMembers = new();
        foreach (var channel in channels)
        {
            var channelGrain = context.ChannelClient.GetGrain<IChannelGrain>(channel.Name);
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
        await File.WriteAllTextAsync(_workspaceChatHistoryPath, json);
    }

    private async Task ShowChannelMembers(ClientContext context)
    {
        var room = context.ChannelClient.GetGrain<IChannelGrain>(context.CurrentChannel);

        if (!context.IsConnectedToChannel)
        {
            AnsiConsole.MarkupLine("[bold red]You are not connected to any channel[/]");
            return;
        }

        var members = await room.GetMembers();

        AnsiConsole.Write(new Rule($"Members for '{context.CurrentChannel}'")
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

    static async Task ShowCurrentRoomMembers(ClientContext context)
    {
        var room = context.ChannelClient.GetGrain<IRoomGrain>(context.CurrentRoom);

        var members = await room.GetMembers();

        AnsiConsole.Write(new Rule($"Members for '{context.CurrentRoom}'")
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

    static async Task ShowCurrentRoomChannels(ClientContext context)
    {
        var room = context.ChannelClient.GetGrain<IRoomGrain>(context.CurrentRoom);

        var channels = await room.GetChannels();

        AnsiConsole.Write(new Rule($"Channels for '{context.CurrentRoom}'")
        {
            Justification = Justify.Center,
            Style = Style.Parse("darkgreen")
        });

        foreach (var channel in channels)
        {
            AnsiConsole.MarkupLine("[bold yellow]{0}[/]", channel);
        }

        AnsiConsole.Write(new Rule()
        {
            Justification = Justify.Center,
            Style = Style.Parse("darkgreen")
        });
    }

    static async Task ShowCurrentChannelHistory(ClientContext context)
    {
        var room = context.ChannelClient.GetGrain<IChannelGrain>(context.CurrentChannel);

        if (!context.IsConnectedToChannel)
        {
            AnsiConsole.MarkupLine("[bold red]You are not connected to any channel[/]");
            return;
        }

        var history = await room.ReadHistory(1_000);

        AnsiConsole.Write(new Rule($"History for '{context.CurrentChannel}'")
        {
            Justification = Justify.Center,
            Style = Style.Parse("darkgreen")
        });

        foreach (var chatMsg in history)
        {
            AnsiConsole.MarkupLine("[[[dim]{0}[/]]] [bold yellow]{1}:[/] {2}",
                chatMsg.Created.LocalDateTime, chatMsg.From!, chatMsg.Text);
        }

        AnsiConsole.Write(new Rule()
        {
            Justification = Justify.Center,
            Style = Style.Parse("darkgreen")
        });
    }

    async Task SendMessage(
        ClientContext context,
        string messageText)
    {
        var message = new ChatMsg(context.UserName!, messageText);
        var room = context.ChannelClient.GetGrain<IChannelGrain>(context.CurrentChannel!);
        await room.Message(message);
    }

    private async Task<ClientContext> JoinChannel(
        ClientContext context,
        string channelName)
    {
        if (context.CurrentChannel is not null &&
            !string.Equals(context.CurrentChannel, channelName, StringComparison.OrdinalIgnoreCase))
        {
            await LeaveChannel(context);
        }

        context = context with { CurrentChannel = channelName };
        await AnsiConsole.Status().StartAsync("Joining channel...", async ctx =>
        {
            var room = context.ChannelClient.GetGrain<IRoomGrain>(context.CurrentRoom);
            await room.CreateChannel(channelName);
            var channel = context.ChannelClient.GetGrain<IChannelGrain>(context.CurrentChannel);
            await channel.JoinChannel(context.UserName!, "Human user", true, _roomObserverRef);
        });
        return context;
    }

    private async Task<ClientContext> LeaveChannel(ClientContext context)
    {
        if (!context.IsConnectedToChannel)
        {
            AnsiConsole.MarkupLine("[bold red]You are not connected to any channel[/]");
            return context;
        }

        await AnsiConsole.Status().StartAsync("Leaving channel...", async ctx =>
        {
            var channel = context.ChannelClient.GetGrain<IChannelGrain>(context.CurrentChannel);
            await channel.LeaveChannel(context.UserName!);
        });

        return context with { CurrentChannel = null };
    }

}
