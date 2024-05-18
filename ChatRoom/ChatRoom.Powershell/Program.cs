// See https://aka.ms/new-console-template for more information
using ChatRoom.Common;
using ChatRoom.Powershell;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using var host = new HostBuilder()
    .UseOrleansClient(clientBuilder =>
    {
        clientBuilder
            .UseLocalhostClustering()
            .AddMemoryStreams("chat");
    })
    .Build();

await host.StartAsync();
var pwsh = new PowershellRunnerAgent("ps-runner");

var client = host.Services.GetRequiredService<IClusterClient>();

var chatPlatformClient = new ChatPlatformClient(client);
await chatPlatformClient.RegisterAgentAsync(pwsh, "A powershell runner that can run pwsh code snippet");
var lifetimeManager = host.Services.GetRequiredService<IHostApplicationLifetime>();
lifetimeManager.ApplicationStopping.Register(async () =>
{
    Console.WriteLine("Unsubscribing from the agent info stream...");
    await chatPlatformClient.UnregisterAgentAsync(pwsh);
});

await host.WaitForShutdownAsync();
