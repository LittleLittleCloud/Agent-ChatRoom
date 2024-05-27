// See https://aka.ms/new-console-template for more information
using ChatRoom.OpenAI;
using Spectre.Console.Cli;

var app = new CommandApp<OpenAICommand>()
    .WithDescription(OpenAICommand.Description);
await app.RunAsync(args);
