using ChatRoom.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;

//var channelHost = Host.CreateDefaultBuilder(args)
//    .UseOrleans(siloBuilder =>
//    {
//        siloBuilder
//            .UseLocalhostClustering()
//            .AddMemoryGrainStorage("PubSubStore")
//            .ConfigureLogging(logBuilder =>
//            {
//                logBuilder
//                    .ClearProviders();
//            });
//    })
//    .ConfigureServices(serviceCollection => serviceCollection.AddHostedService<ConsoleChatRoomService>())
//    .Build();

//await channelHost.StartAsync();
//await channelHost.WaitForShutdownAsync();

var app = new CommandApp<ChatRoomClientCommand>();
await app.RunAsync(args);
