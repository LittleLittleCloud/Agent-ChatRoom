using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console.Cli;

namespace ChatRoom.Client;

internal class ChatRoomClientSettings : CommandSettings
{
    [Description("The room name to create.")]
    [CommandOption("-r|--room <ROOM>")]
    [DefaultValue("room")]
    public string? Room { get; init; } = "room";

    [Description("The port to listen.")]
    [CommandOption("-p|--port <PORT>")]
    [DefaultValue(30000)]
    public int Port { get; init; } = 30000;

    [Description("Your name in the room")]
    [CommandOption("-n|--name <NAME>")]
    public string YourName { get; init; } = "User";

    [Description("Configuration file")]
    [CommandOption("-c|--config <CONFIG>")]
    public string? ConfigFile { get; init; } = null;
}
