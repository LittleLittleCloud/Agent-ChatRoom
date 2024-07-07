// See https://aka.ms/new-console-template for more information
using ChatRoom.OpenAI;
using Spectre.Console.Cli;

var app = CommandFactory.Create();
await app.RunAsync(args);
