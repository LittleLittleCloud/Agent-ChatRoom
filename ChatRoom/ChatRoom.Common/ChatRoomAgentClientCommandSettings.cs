using System.ComponentModel;
using Spectre.Console.Cli;

namespace ChatRoom.SDK;
internal class ChatRoomAgentClientCommandSettings : CommandSettings
{
    [Description("Configuration file.")]
    [CommandOption("-c|--config <CONFIG>")]
    public string? ConfigFile { get; init; }
}

internal abstract class ChatRoomAgentCommand : AsyncCommand<ChatRoomAgentClientCommandSettings>
{}
