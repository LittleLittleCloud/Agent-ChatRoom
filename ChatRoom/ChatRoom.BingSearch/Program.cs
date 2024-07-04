using Spectre.Console.Cli;

var app = new CommandApp<WebSearchCommand>();
await app.RunAsync(args);

