using ChatRoom.Github;
using Spectre.Console.Cli;

var app = new CommandApp<GithubCommand>()
    .WithDescription(GithubCommand.Description);

await app.RunAsync(args);
