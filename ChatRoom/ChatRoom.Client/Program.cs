using ChatRoom.Client;
using Microsoft.AspNetCore.Components.Forms;
using Spectre.Console.Cli;

var app = new CommandApp();
app.Configure(config =>
{
    config.AddCommand<ChatRoomClientCommand>("run")
        .WithDescription(ChatRoomClientCommand.Description)
        .WithExample(["run", "-c", "config.json"]);

    config.AddCommand<CreateConfigurationCommand>("create")
        .WithDescription("Create a configuration file for ChatRoom.Client")
        .WithExample(["create", "--template", "chatroom"]);
});
await app.RunAsync(args);
