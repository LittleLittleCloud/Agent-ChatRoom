using ChatRoom.Client;
using Spectre.Console.Cli;

var app = new CommandApp<ChatRoomClientCommand>();
await app.RunAsync(args);
