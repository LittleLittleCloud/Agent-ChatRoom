using System.ComponentModel;
using Spectre.Console.Cli;

namespace ChatRoom.SDK;
public abstract class ChatRoomAgentClientCommandSettings : CommandSettings
{
    [Description("The room name to create.")]
    [CommandOption("-r|--room <ROOM>")]
    public string? Room { get; init; } = null;

    [Description("The port to listen.")]
    [CommandOption("-p|--port <PORT>")]
    public int? Port { get; init; } = null;

    [Description("Configuration file")]
    [CommandOption("-c|--config <CONFIG>")]
    public abstract string? ConfigFile { get; init; }
}

public abstract class ChatRoomAgentCommand : AsyncCommand<ChatRoomAgentClientCommandSettings>
{}
