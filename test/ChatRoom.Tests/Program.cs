using System.Management.Automation;
using ChatRoom.WebSearch.Tests;
using ChatRoom.Client.Tests;
using ChatRoom.Github.Tests;
using ChatRoom.OpenAI.Tests;
using ChatRoom.Powershell.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ChatRoom.SDK;
using Moq;
using AutoGen.Core;

using var defaultClientFixture = new EmptyChatRoomClientFixture();
using var openaiFixture = new OpenAIAgentsFixture();
using var bing = new WebSearchFixture();
using var githubFixture = new GithubAgentsFixture();
using var psFixture = new PowershellAgentsFixture();
using var host = Host.CreateDefaultBuilder()
    .UseChatRoomClient(defaultClientFixture.Configuration.RoomConfig.Room, port: defaultClientFixture.Configuration.RoomConfig.Port)
    .Build();

var applicationLifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();

applicationLifetime.ApplicationStopping.Register(() =>
{
    Console.WriteLine("Application is stopping.");
});

await host.StartAsync();

var client = host.Services.GetRequiredService<ChatPlatformClient>();

var throwExceptionAgent = Mock.Of<IAgent>();
Mock.Get(throwExceptionAgent)
            .Setup(a => a.GenerateReplyAsync(It.IsAny<IEnumerable<IMessage>>(), It.IsAny<GenerateReplyOptions>(), It.IsAny<CancellationToken>()))
            .Throws(() =>
            {
                Task.Delay(35 * 1000).Wait();

                return new Exception("Test exception");
            });

Mock.Get(throwExceptionAgent)
    .Setup(a => a.Name)
    .Returns(nameof(throwExceptionAgent));

await client.RegisterAutoGenAgentAsync(throwExceptionAgent);

await host.WaitForShutdownAsync();
