using ChatRoom.SDK;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using weather_agent;

var roomConfig = new RoomConfiguration
{
    Room = "room",
    Port = 30000,
};

var serverConfig = new ChatRoomServerConfiguration
{
    RoomConfig = roomConfig,
    YourName = "User",
    ServerConfig = new ServerConfiguration
    {
        Urls = "http://localhost:50001",
    },
};

using var host = Host.CreateDefaultBuilder()
    .UseChatRoomServer(serverConfig)
    .Build();

await host.StartAsync();

var client = host.Services.GetRequiredService<ChatPlatformClient>();
var agent = WeatherAgentFactory.CreateAgent(
    name: "weather_agent",
    modelName: "gpt-3.5-turbo");

// add weather agent to chatroom
await client.RegisterAutoGenAgentAsync(agent, "A weather report agent.");

// get all orchestrators
var orchestrators = await client.GetOrchestrators();

// create a weather channel
await client.CreateChannel("weather", ["weather_agent", serverConfig.YourName], orchestrators: orchestrators);

await host.WaitForShutdownAsync();
