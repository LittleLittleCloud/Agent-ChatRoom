using Azure.AI.OpenAI;
using Google.Cloud.AIPlatform.V1;
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
        return hostBuilder.UseChatRoomClient(new RoomConfiguration
        {
            Room = roomName,
            Port = port
        });
    }

    /// <summary>
    /// Add chatroom client and <see cref="ChatPlatformClient"/> to host builder.
    /// Use this when the chatroom server is hosted in a separate process.
    /// </summary>
    /// <param name="hostBuilder"></param>
    /// <param name="roomConfig"></param>
    public static IHostBuilder UseChatRoomClient(
        this IHostBuilder hostBuilder,
        RoomConfiguration roomConfig)
    {
        return hostBuilder
            .UseOrleansClient(clientBuilder =>
            {
                clientBuilder
                    .UseLocalhostClustering(gatewayPort: roomConfig.Port);
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
                        room: roomConfig.Room,
                        lifecycleService: lifecycle,
                        logger: logger);
                });
            });
    }

    /// <summary>
    /// Add Agent chatroom and <see cref="ChatPlatformClient"/> to host builder.
    /// This will start a in-process chatroom server and add the <see cref="ChatPlatformClient"/> to the host builder.
    /// 
    /// <para>
    /// This will also add the following orchestrators:
    /// </para>
    /// <item><see cref="HumanToAgent"/></item>
    /// 
    /// <para>
    /// if <paramref name="openAIConfig"/> is provided, the following orchestrators will be added:
    /// </para>
    /// <list type="bullet">
    /// <item><see cref="DynamicGroupChat"/></item>
    /// <item><see cref="RoundRobin"/></item>
    /// </list>
    /// Use this when you want to host the chatroom server in the same process.
    /// </summary>
    /// <param name="hostBuilder"></param>
    /// <param name="roomConfig"></param>
    /// <param name="openAIConfig">the configuration for <see cref="OpenAIClient"/>, if provided,
    /// <see cref="HumanToAgent"/> and <see cref="DynamicGroupChat"/> orchestrator will be added to the chatroom server.</param>
    /// <returns></returns>
    internal static IHostBuilder UseChatRoomServer(
        this IHostBuilder hostBuilder,
        RoomConfiguration roomConfig,
        OpenAIClientConfiguration? openAIConfig = null)
    {
        return hostBuilder
            .UseOrleans(siloBuilder =>
            {
                siloBuilder
                     .UseLocalhostClustering(gatewayPort: roomConfig.Port)
                     .AddMemoryGrainStorage("PubSubStore");
            })
            .ConfigureServices((ctx, serviceCollections) =>
            {
                serviceCollections.AddSingleton<RoundRobin>();

                if (openAIConfig != null)
                {
                    serviceCollections.AddSingleton(sp =>
                    {
                        return new HumanToAgent(openAIConfig);
                    });
                    serviceCollections.AddSingleton(sp =>
                    {
                        return new DynamicGroupChat(openAIConfig);
                    });
                }

                serviceCollections.AddSingleton(sp =>
                {
                    var client = sp.GetRequiredService<IClusterClient>();
                    var lifecycle = sp.GetService<IHostApplicationLifetime>();
                    var logger = sp.GetService<ILogger<ChatPlatformClient>>();
                    return new ChatPlatformClient(
                        client: client,
                        room: roomConfig.Room,
                        lifecycleService: lifecycle,
                        logger: logger);
                });
            });
    }
}
