using ChatRoom.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;

var app = new CommandApp<ChatRoomClientCommand>();
await app.RunAsync(args);
