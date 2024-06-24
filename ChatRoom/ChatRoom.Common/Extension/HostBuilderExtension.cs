using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ChatRoom.SDK;

public static class HostBuilderExtension
{
    /// <summary>
    /// Add chatroom client and <see cref="ChatPlatformClient"/> to host builder.
    /// Use this when the chatroom server is hosted in a separate process.
    /// </summary>
    /// <param name="hostBuilder"></param>
    /// <param name="roomName">room name.</param>
    /// <param name="port">the port of the gateway.</param>
    public static IHostBuilder UseChatRoomClient(
        this IHostBuilder hostBuilder,
        string roomName = "room",
        int port = 30000)
    {
        return hostBuilder
            .UseOrleansClient(clientBuilder =>
            {
                clientBuilder
                    .UseLocalhostClustering(gatewayPort: port);
            })
            .ConfigureServices((ctx, serviceCollections) =>
            {
                serviceCollections.AddSingleton(sp =>
                {
                    var client = sp.GetRequiredService<IClusterClient>();
                    var lifecycle = sp.GetService<IHostApplicationLifetime>();
                    var logger = sp.GetService<ILogger<ChatPlatformClient>>();
                    return new ChatPlatformClient(
                        client: client,
                        room: roomName,
                        lifecycleService: lifecycle,
                        logger: logger);
                });
            });
    }

    /// <summary>
    /// Add Agent chatroom and <see cref="ChatPlatformClient"/> to host builder.
    /// This will start a in-process chatroom server and add the <see cref="ChatPlatformClient"/> to the host builder.
    /// Use this when you want to host the chatroom server in the same process.
    /// </summary>
    /// <param name="hostBuilder"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    public static IHostBuilder UseChatRoomServer(
        this IHostBuilder hostBuilder,
        RoomConfiguration config)
    {
        return hostBuilder
            .UseOrleans(siloBuilder =>
            {
                siloBuilder
                     .UseLocalhostClustering(gatewayPort: config.Port)
                     .AddMemoryGrainStorage("PubSubStore");
            })
            .ConfigureServices((ctx, serviceCollections) =>
            {
                serviceCollections.AddSingleton(sp =>
                {
                    var client = sp.GetRequiredService<IClusterClient>();
                    var lifecycle = sp.GetService<IHostApplicationLifetime>();
                    var logger = sp.GetService<ILogger<ChatPlatformClient>>();
                    return new ChatPlatformClient(
                        client: client,
                        room: config.Room,
                        lifecycleService: lifecycle,
                        logger: logger);
                });
            });
    }
}
