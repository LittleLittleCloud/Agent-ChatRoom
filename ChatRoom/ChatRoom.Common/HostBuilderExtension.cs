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
                serviceCollections.UseChatRoom(roomName, port);
            });
    }

    public static IHostBuilder AddAgentAsync<TAgent>(
        this IHostBuilder hostBuilder,
        Func<IServiceProvider, Task<TAgent>> agentFactory,
        string selfDescription)
        where TAgent : IAgent
    {
        return hostBuilder
            .ConfigureServices(async (ctx, serviceCollections) =>
            {
                serviceCollections.AddSingleton(sp =>
                {
                    var agent = agentFactory(sp).Result;
                    var agentInfo = new AgentInfo(agent.Name, selfDescription);
                    return new AgentInfoAgent(agent, agentInfo);
                });
            });
    }

    public static async Task WaitForAgentsJoinRoomAsync(this IHost host)
    {
        var collections = host.Services.GetServices<AgentInfoAgent>();
        foreach (var item in collections)
        {
            var agentInfo = item.Info;
            var agent = item.Agent;
            await host.JoinRoomAsync(agent, agentInfo.SelfDescription);
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
