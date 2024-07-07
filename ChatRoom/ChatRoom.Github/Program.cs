using ChatRoom.Github;
using Spectre.Console.Cli;
var app = new CommandApp();

app.Configure(config =>
{
    config.AddCommand<GithubCommand>("run")
        .WithDescription(GithubCommand.Description)
        .WithExample(["run", "-c", "chatroom-github.json"]);
    config.AddCommand<CreateConfigurationCommand>("create")
        .WithDescription("Create a configuration file for ChatRoom.Github")
        .WithExample(["create", "--template", "chatroom-github"]);
});

await app.RunAsync(args);
