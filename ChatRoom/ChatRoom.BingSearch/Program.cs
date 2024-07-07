using ChatRoom.WebSearch;
using Spectre.Console.Cli;
var app = new CommandApp();
app.Configure(config =>
{
    config.AddCommand<WebSearchCommand>("run")
        .WithDescription("Run the web search agent.")
        .WithExample(["run", "-c", "chatroom-websearch.json"]);

    config.AddCommand<CreateConfigurationCommand>("create")
        .WithDescription("Create a configuration file for ChatRoom.WebSearch")
        .WithExample(["create", "--template", "chatroom-websearch"]);
});
await app.RunAsync(args);

