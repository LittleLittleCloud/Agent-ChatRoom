using AutoGen.Core;
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
    modelName: "gpt-4o-mini");

// the user agent's name must match with ChatRoomServerConfiguration.YourName field.
// When chatroom starts, it will be replaced by a built-in user agent.
var userAgent = new DefaultReplyAgent("User", "<dummy>");

var groupChat = new GroupChat([userAgent, agent]);

// add weather groupchat to chatroom
await client.RegisterAutoGenGroupChatAsync("weather-chat", groupChat);
await host.WaitForShutdownAsync();
