using System.Reflection;
using ChatRoom.Common;
using Microsoft.Extensions.Hosting;
using Orleans.Runtime;
using Spectre.Console;

namespace ChatRoom.Client;

public class ConsoleChatRoomService : IHostedService
{
    private readonly IClusterClient _clusterClient;
    private readonly ClientContext _clientContext;
    private Task? _processTask = null;
    private readonly ConsoleRoomObserver _roomObserver;
    private readonly IRoomObserver _roomObserverRef;
    private readonly Dictionary<string, IChannelObserver> _channelObservers = new();

    public ConsoleChatRoomService(IClusterClient clsterClient)
    {
        _roomObserver = new ConsoleRoomObserver();
        _roomObserverRef = clsterClient.CreateObjectReference<IRoomObserver>(_roomObserver);
        _clusterClient = clsterClient;
        _clientContext = new ClientContext(_clusterClient, UserName: "Zhang", CurrentChannel: "General", CurrentRoom: "room");
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        PrintUsage();
        var room = _clientContext.ChannelClient.GetGrain<IRoomGrain>(_clientContext.CurrentRoom);
        await room.Join(_clientContext.AgentInfo!, _roomObserverRef);
        await JoinChannel(_clientContext, _clientContext.CurrentChannel!);
        _processTask = ProcessLoopAsync(_clientContext, cancellationToken);
    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        //_processTask?.Wait();
        AnsiConsole.MarkupLine("[bold red]Exiting...[/]");
        _processTask = null;
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
                    
                    await room.AddAgentToChannel(channelInfo, memberInfo);
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
                    await room.RemoveAgentFromChannel(channelInfo, memberInfo);

                    continue;
                }
            }

            var firstTwoCharacters = input.Length >= 2 ? input[..2] : string.Empty;
            if (firstTwoCharacters is "/n")
            {
                context = context with { UserName = input.Replace("/n", "").Trim() };
                AnsiConsole.MarkupLine(
                    "[dim][[STATUS]][/] Set username to [lime]{0}[/]", context.UserName);
                continue;
            }

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
        using var logoStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ChatRoom.Client.logo.png");
        var logo = new CanvasImage(logoStream!)
        {
            MaxWidth = 25
        };

        var table = new Table()
        {
            Border = TableBorder.None,
            Expand = true,
        }.HideHeaders();
        table.AddColumn(new TableColumn("One"));

        var header = new FigletText("Agent Chat Room")
        {
            Color = Color.Aqua
        };

        var markup = new Markup(
           "[bold fuchsia]/j[/] [aqua]<channel>[/] to [underline green]join[/] a specific channel\n"
           + "[bold fuchsia]/n[/] [aqua]<username>[/] to set your [underline green]name[/]\n"
           + "[bold fuchsia]/l[/] to [underline green]leave[/] the current channel\n"
           + "[bold fuchsia]/h[/] to re-read channel [underline green]history[/]\n"
           + "[bold fuchsia]/m[/] to query [underline green]members[/] in the current channel\n"
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
            .AddEmptyRow()
            .AddEmptyRow()
            .AddRow(markup);
        table.AddRow(logo, rightTable);

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
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
            var channel = context.ChannelClient.GetGrain<IChannelGrain>(context.CurrentChannel);
            await channel.Join(context.AgentInfo!, _roomObserverRef);
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
            var room = context.ChannelClient.GetGrain<IChannelGrain>(context.CurrentChannel);
            var reference = _channelObservers[context.CurrentChannel!];
            await room.Leave(context.AgentInfo!);
            _channelObservers.Remove(context.CurrentChannel!);
        });

        return context with { CurrentChannel = null };
    }

}
