// See https://aka.ms/new-console-template for more information
using Azure.AI.OpenAI;
using ChatRoom;
using ChatRoom.Powershell;
using ChatRoom.SDK;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;

var app = new CommandApp<PowershellCommand>();
await app.RunAsync(args);
