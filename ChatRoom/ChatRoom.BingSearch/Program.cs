using Spectre.Console.Cli;

var app = new CommandApp<BingSearchCommand>();
await app.RunAsync(args);

