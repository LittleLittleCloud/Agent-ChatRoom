using ChatRoom.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var channelHost = Host.CreateDefaultBuilder(args)
    .UseOrleans(siloBuilder =>
    {
        siloBuilder
            .UseLocalhostClustering()
            .AddMemoryGrainStorage("PubSubStore")
            .AddMemoryStreams("chat")
            .ConfigureLogging(logBuilder =>
            {
                logBuilder
                .ClearProviders();
                //.AddFilter((loglevel) => loglevel > LogLevel.Information);
            });
    })
    .ConfigureServices(serviceCollection => serviceCollection.AddHostedService<ConsoleChatRoomService>())
    .Build();

await channelHost.StartAsync();
await channelHost.WaitForShutdownAsync();
