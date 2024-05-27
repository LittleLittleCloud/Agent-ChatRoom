using ChatRoom.Client;
using Spectre.Console.Cli;

var app = new CommandApp<ChatRoomClientCommand>()
    .WithDescription(ChatRoomClientCommand.Description);
await app.RunAsync(args);
