using System.Management.Automation;
using ChatRoom.WebSearch.Tests;
using ChatRoom.Client.Tests;
using ChatRoom.Github.Tests;
using ChatRoom.OpenAI.Tests;
using ChatRoom.Powershell.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using var defaultClientFixture = new DefaultClientFixture();
using var openaiFixture = new OpenAIAgentsFixture();
using var bing = new WebSearchFixture();
using var githubFixture = new GithubAgentsFixture();
using var psFixture = new PowershellAgentsFixture();
using var host = Host.CreateDefaultBuilder()
    .Build();

var applicationLifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();

applicationLifetime.ApplicationStopping.Register(() =>
{
    Console.WriteLine("Application is stopping.");
});

await host.StartAsync();
await host.WaitForShutdownAsync();
