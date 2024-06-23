using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoGen.Core;
using ChatRoom.SDK;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ChatRoom.SDK;

public static class HostBuilderExtension
{
    /// <summary>
    /// Add Agent ChatRoom to the host builder.
    /// This will add the <see cref="ChatPlatformClient"/> to the service collection.
    /// </summary>
    /// <param name="hostBuilder"></param>
    /// <param name="roomName">room name.</param>
    /// <param name="port">the port of the gateway.</param>
    public static IHostBuilder UseChatRoom(
        this IHostBuilder hostBuilder,
        string roomName = "room",
        int port = 30000)
    {
        return hostBuilder
            .ConfigureServices((ctx, serviceCollections) =>
            {
                serviceCollections.UseChatRoom(roomName, port);
            });
    }

    /// <inheritdoc cref="UseChatRoom(IHostBuilder, string, int)"/>
    public static IServiceCollection UseChatRoom(
        this IServiceCollection collection,
        string roomName = "room",
        int port = 30000)
    {
        collection.AddOrleansClient(clientBuilder =>
            {
                clientBuilder
                    .UseLocalhostClustering(gatewayPort: port);
            })
            .AddSingleton(sp =>
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

        return collection;
    }
}
