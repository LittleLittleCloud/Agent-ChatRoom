// See https://aka.ms/new-console-template for more information
using Azure.AI.OpenAI;
using ChatRoom;
using ChatRoom.BingSearch;
using ChatRoom.Common;
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
Console.WriteLine("Client is connected to the server.");
var agent = AgentFactory.CreateBingSearchAgent();


//// join the General channel
var roomName = "General";
var channel = client.GetGrain<IChannelGrain>(roomName);
var agentInfo = new AgentInfo(agent.Name, "bing search agent");
var streamId = await channel.Join(agentInfo);
// subscribe to the chat stream
var streamProvider = client.GetStreamProvider("chat");
var stream = streamProvider.GetStream<AgentInfo>(
       StreamId.Create("AgentInfo", "General"));
Console.WriteLine("Subscribing to the agent info stream...");
var observer = new NextAgentStreamObserver(agent, channel);
await stream.SubscribeAsync(observer);

await host.WaitForShutdownAsync();

await channel.Leave(agentInfo);
