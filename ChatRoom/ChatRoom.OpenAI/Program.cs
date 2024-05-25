// See https://aka.ms/new-console-template for more information
using ChatRoom.OpenAI;
using Spectre.Console.Cli;

var app = new CommandApp<OpenAICommand>();
await app.RunAsync(args);
