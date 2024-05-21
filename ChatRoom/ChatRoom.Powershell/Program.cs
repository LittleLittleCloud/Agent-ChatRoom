// See https://aka.ms/new-console-template for more information
using ChatRoom.Common;
using ChatRoom.Powershell;
using ChatRoom.SDK;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
var pwsh = new PowershellRunnerAgent("ps-runner");
using var host = Host.CreateDefaultBuilder(args)
    .UseChatRoom()
    .Build();

await host.StartAsync();
await host.JoinRoomAsync(pwsh);
await host.WaitForShutdownAsync();
