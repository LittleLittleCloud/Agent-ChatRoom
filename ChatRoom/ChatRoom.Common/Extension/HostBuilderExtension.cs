using System.Reflection;
using Azure.AI.OpenAI;
using Google.Cloud.AIPlatform.V1;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

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
    /// If ChatRoom.StaticWebUI is added to the project, it will also start a web ui.
    /// <para>
    /// This will also add the following orchestrators:
    /// </para>
    /// <item><see cref="HumanToAgent"/></item>
    /// <para>
    /// </para>
    /// 
    /// <para>If the <see cref="ChannelConfiguration.OpenAIConfiguration"/> in <paramref name="serverConfig"/>
    /// is not null, the following orchestrators will also be added:</para>
    /// <item><see cref="DynamicGroupChat"/></item>
    /// <item><see cref="RoundRobin"/></item>
    /// Use this when you want to host the chatroom server in the same process.
    /// </summary>
    /// <param name="hostBuilder"></param>
    /// <param name="serverConfig"></param>
    /// <returns></returns>
    public static IHostBuilder UseChatRoomServer(
        this IHostBuilder hostBuilder,
        ChatRoomServerConfiguration serverConfig)
    {
        hostBuilder
            .UseOrleans(siloBuilder =>
            {
                siloBuilder
                     .UseLocalhostClustering(gatewayPort: serverConfig.RoomConfig.Port)
                     .AddMemoryGrainStorage("PubSubStore");
            })
            .ConfigureServices((ctx, serviceCollections) =>
            {
                serviceCollections.AddSingleton(serverConfig);
                serviceCollections.AddSingleton(serverConfig.RoomConfig);
                serviceCollections.AddSingleton(serverConfig.ChannelConfig);
                serviceCollections.AddHostedService<AgentExtensionBootstrapService>();
                serviceCollections.AddSingleton<ConsoleRoomAgent>();
                serviceCollections.AddSingleton<RoundRobin>();

                if (serverConfig.ChannelConfig.OpenAIConfiguration is OpenAIClientConfiguration openAIConfig)
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

                if (serverConfig.EnableConsoleApp)
                {
                    serviceCollections.AddSingleton<ChatRoomConsoleApp>();
                }

                serviceCollections.AddSingleton(sp =>
                {
                    var client = sp.GetRequiredService<IClusterClient>();
                    var lifecycle = sp.GetService<IHostApplicationLifetime>();
                    var logger = sp.GetService<ILogger<ChatPlatformClient>>();
                    var observer = sp.GetRequiredService<ConsoleRoomAgent>();
                    var roudRobinOrchestrator = sp.GetRequiredService<RoundRobin>();
                    var humanToAgent = sp.GetService<HumanToAgent>();
                    var dynamicGroupChat = sp.GetService<DynamicGroupChat>();
                    var chatroomClient = new ChatPlatformClient(
                        client: client,
                        room: serverConfig.RoomConfig.Room,
                        lifecycleService: lifecycle,
                        logger: logger);

                    chatroomClient.RegisterOrchestratorAsync("RoundRobin", roudRobinOrchestrator).Wait();
                    
                    if (humanToAgent is not null)
                    {
                        chatroomClient.RegisterOrchestratorAsync("HumanToAgent", humanToAgent).Wait();
                    }

                    if (dynamicGroupChat is not null)
                    {
                        chatroomClient.RegisterOrchestratorAsync("DynamicGroupChat", dynamicGroupChat).Wait();
                    }
                    chatroomClient.RegisterAgentAsync(serverConfig.YourName, "Human User", true, observer).Wait();

                    return chatroomClient ?? throw new InvalidOperationException("Failed to get ChatPlatformClient.");
                });
            });

        // configure web host
        if (serverConfig.ServerConfig is ServerConfiguration serverConfiguration)
        {
            hostBuilder.ConfigureWebHostDefaults(builder =>
            {
                var assemblyLocation = Assembly.GetExecutingAssembly().Location;
                var assemblyDirectory = Path.GetDirectoryName(assemblyLocation) ?? Environment.CurrentDirectory;
                var webRoot = Path.Combine(assemblyDirectory, "wwwroot");
                
                // check if index.html exists in the wwwroot folder
                if (!File.Exists(Path.Combine(webRoot, "index.html")))
                {
                    Console.WriteLine("index.html not found in wwwroot folder.");
                    Console.WriteLine("Please add ChatRoom.StaticWebUI package to your project if you want to use the default web UI.");
                }
                
                builder
                .UseWebRoot(webRoot)
                .UseContentRoot(serverConfig.Workspace)
                .UseEnvironment(serverConfiguration.Environment)
                .UseUrls(serverConfiguration.Urls)
                .UseStartup<WebHostStartup>();
            });
        }

        return hostBuilder;
    }
}
