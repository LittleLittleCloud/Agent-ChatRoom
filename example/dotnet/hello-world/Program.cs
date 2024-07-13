

using AutoGen.Core;
using ChatRoom.SDK;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

#region start_chatroom_server
// program.cs
var serverConfig = new ChatRoomServerConfiguration
{
    RoomConfig = new RoomConfiguration
    {
        Room = "room",
        Port = 30000,
    },
    YourName = "User",
    // provide the server configuration for in-proc chatroom server
    ServerConfig = new ServerConfiguration
    {
        Urls = "http://localhost:50001",
    },
};

using var host = Host.CreateDefaultBuilder()
    .UseChatRoomServer(serverConfig)
    .Build();

await host.StartAsync();
#endregion

#region get_chatroom_client
// program.cs
var client = host.Services.GetRequiredService<ChatPlatformClient>();
var agent = new HelloWorldAgent("hello_world");

await client.RegisterAutoGenAgentAsync(agent, "A hello world agent.");
await host.WaitForShutdownAsync();
#endregion

#region hello_world_agent
// program.cs
public class HelloWorldAgent : IAgent
{
    public HelloWorldAgent(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public async Task<IMessage> GenerateReplyAsync(IEnumerable<IMessage> messages, GenerateReplyOptions? options = null, CancellationToken cancellationToken = default)
    {
        return new TextMessage(Role.Assistant, "Hello, World!", from: this.Name);
    }
}
#endregion
