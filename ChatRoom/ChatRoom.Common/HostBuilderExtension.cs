using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoGen.Core;
using ChatRoom.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ChatRoom.SDK;

public static class HostBuilderExtension
{
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
            .AddSingleton<ChatPlatformClient>(sp =>
            {
                var client = sp.GetRequiredService<IClusterClient>();
                return new ChatPlatformClient(client, roomName);
            });

        return collection;
    }

    public static IHostBuilder UseChatRoom(
        this IHostBuilder hostBuilder)
    {
        return hostBuilder
            .ConfigureServices((ctx, serviceCollections) =>
            {
                var configuration = ctx.Configuration;
                var port = configuration.GetValue<int?>("Port") ?? 30000;
                var roomName = configuration.GetValue<string?>("Room") ?? "room";

                serviceCollections.AddSingleton<AgentCollectionService>();
                serviceCollections.UseChatRoom(roomName, port);
            });
    }

    public static IHostBuilder AddAgentAsync<TAgent>(
        this IHostBuilder hostBuilder,
        Func<IServiceProvider, Task<TAgent>> agentFactory)
        where TAgent : IAgent
    {
        return hostBuilder
            .ConfigureServices(async (ctx, serviceCollections) =>
            {
                serviceCollections.AddSingleton<IAgent>(sp =>
                {
                    var agentCollection = sp.GetRequiredService<AgentCollectionService>();
                    var agent = agentFactory(sp).Result;

                    return agent;
                });
            });
    }

    public static async Task WaitForAgentsJoinRoomAsync(this IHost host)
    {
        var agentCollection = host.Services.GetServices<IAgent>();
        foreach (var agent in agentCollection)
        {
            await host.JoinRoomAsync(agent);
        }
    }

    public static async Task JoinRoomAsync(
        this IHost host,
        IAgent agent,
        string description = "You are a helpful AI assistant")
    {
        var chatPlatformClient = host.Services.GetRequiredService<ChatPlatformClient>();
        var lifecycle = host.Services.GetRequiredService<IHostApplicationLifetime>();
        await chatPlatformClient.RegisterAgentAsync(agent, description);

        lifecycle.ApplicationStopping.Register(async () =>
        {
            await chatPlatformClient.UnregisterAgentAsync(agent);
        });
    }
}
