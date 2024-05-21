using ChatRoom.BingSearch;
using ChatRoom.SDK;
using Microsoft.Extensions.Hosting;
var agent = AgentFactory.CreateBingSearchAgent();
using var host = new HostBuilder()
    .UseChatRoom()
    .Build();
await host.StartAsync();
await host.JoinRoomAsync(agent);
await host.WaitForShutdownAsync();
