﻿// See https://aka.ms/new-console-template for more information
using Azure.AI.OpenAI;
using ChatRoom;
using ChatRoom.Powershell;
using ChatRoom.SDK;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;
var app = new CommandApp();

app.Configure(app =>
{
    app.AddCommand<CreateConfigurationCommand>("create")
        .WithDescription("Create and save a configuration file from given template.")
        .WithExample(["create", "--template", "chatroom-powershell"]);

    app.AddCommand<ChatRoomAgentCommand>("run")
        .WithDescription("Run the chat room agent.")
        .WithExample(["run", "-c", "config.json"]);
});

await app.RunAsync(args);
