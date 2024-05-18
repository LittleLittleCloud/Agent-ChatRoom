using ChatRoom;
using ChatRoom.BingSearch;
using ChatRoom.Common;
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

var client = host.Services.GetRequiredService<IClusterClient>();

await host.StartAsync();
var agent = AgentFactory.CreateBingSearchAgent();
var agentInfo = new AgentInfo(agent.Name, "I am Bing Search Agent, I am good at searching the web.", false);
var chatPlatformClient = new ChatPlatformClient(client);

await chatPlatformClient.RegisterAgentAsync(agent, agentInfo.SelfDescription);

var lifetimeManager = host.Services.GetRequiredService<IHostApplicationLifetime>();

lifetimeManager.ApplicationStopping.Register(async () =>
{
    Console.WriteLine("Unsubscribing from the agent info stream...");
    await chatPlatformClient.UnregisterAgentAsync(agent);
});

await host.WaitForShutdownAsync();
