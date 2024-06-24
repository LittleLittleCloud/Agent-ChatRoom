using ChatRoom.SDK;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using weather_agent;

var roomConfig = new RoomConfiguration
{
    Room = "room",
    Port = 30000,
};

using var host = Host.CreateDefaultBuilder()
    .UseChatRoomClient(roomConfig: roomConfig)
    .Build();

await host.StartAsync();

var client = host.Services.GetRequiredService<ChatPlatformClient>();
var agent = WeatherAgentFactory.CreateAgent(
    name: "weather_agent",
    modelName: "gpt-3.5-turbo");

await client.RegisterAutoGenAgentAsync(agent, "A weather report agent.");
await host.WaitForShutdownAsync();

