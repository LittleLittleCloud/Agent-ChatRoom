using ChatRoom.Client;
using ChatRoom.SDK.Extension;
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

    config.AddListTemplateCommand<ListTemplatesCommand>();
});
await app.RunAsync(args);
