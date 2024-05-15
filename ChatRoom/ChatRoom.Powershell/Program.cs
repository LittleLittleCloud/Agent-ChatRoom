// See https://aka.ms/new-console-template for more information
using ChatRoom;
using ChatRoom.Common;
using ChatRoom.Powershell;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.Runtime;

using var host = new HostBuilder()
    .UseOrleansClient(clientBuilder =>
    {
        clientBuilder
            .UseLocalhostClustering()
            .AddMemoryStreams("chat");
    })
    .Build();

var client = host.Services.GetRequiredService<IClusterClient>();

await host.StartAsync();

// join the General channel
var roomName = "General";
var channel = client.GetGrain<IChannelGrain>(roomName);
var agentInfo = new AgentInfo("ps-runner", "A powershell runner that can run pwsh code snippet", false);
var pwsh = new PowershellRunnerAgent(agentInfo.Name);
var streamId = await channel.Join(agentInfo);
// subscribe to the chat stream
var streamProvider = client.GetStreamProvider("chat");
var stream = streamProvider.GetStream<AgentInfo>(
       StreamId.Create("AgentInfo", "General"));
Console.WriteLine("Subscribing to the agent info stream...");
var observer = new NextAgentStreamObserver(pwsh, channel);
await stream.SubscribeAsync(observer);

// listen for control + c to exit
Console.CancelKeyPress += async (sender, e) =>
{
    await channel.Leave(agentInfo);
};

await host.WaitForShutdownAsync();
