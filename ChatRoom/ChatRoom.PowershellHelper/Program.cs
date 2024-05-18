// See https://aka.ms/new-console-template for more information
using Azure.AI.OpenAI;
using ChatRoom;
using ChatRoom.Common;
using ChatRoom.PowershellHelper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using var host = new HostBuilder()
    .UseOrleansClient(clientBuilder =>
    {
        clientBuilder
            .UseLocalhostClustering()
            .AddMemoryStreams("chat");
    })
    .Build();
await host.StartAsync();

var client = host.Services.GetRequiredService<IClusterClient>();
var lifetimeManager = host.Services.GetRequiredService<IHostApplicationLifetime>();

// create agents
var AZURE_OPENAI_ENDPOINT = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
var AZURE_OPENAI_KEY = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
var AZURE_DEPLOYMENT_NAME = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOY_NAME");
var OPENAI_API_KEY = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
var OPENAI_MODEL_ID = Environment.GetEnvironmentVariable("OPENAI_MODEL_ID") ?? "gpt-3.5-turbo-0125";

OpenAIClient openaiClient;
bool useAzure = false;
if (AZURE_OPENAI_ENDPOINT is string && AZURE_OPENAI_KEY is string && AZURE_DEPLOYMENT_NAME is string)
{
    openaiClient = new OpenAIClient(new Uri(AZURE_OPENAI_ENDPOINT), new Azure.AzureKeyCredential(AZURE_OPENAI_KEY));
    useAzure = true;
}
else if (OPENAI_API_KEY is string)
{
    openaiClient = new OpenAIClient(OPENAI_API_KEY);
}
else
{
    throw new ArgumentException("Please provide either (AZURE_OPENAI_ENDPOINT, AZURE_OPENAI_KEY, AZURE_DEPLOYMENT_NAME) or OPENAI_API_KEY");
}

var deployModelName = useAzure ? AZURE_DEPLOYMENT_NAME! : OPENAI_MODEL_ID;
var agentInfo = new AgentInfo("ps-gpt", "I am PowerShell GPT, I am good at writing powershell scripts.", false);
var pwshDeveloper = AgentFactory.CreatePwshDeveloperAgent(openaiClient, Environment.CurrentDirectory, name: agentInfo.Name, modelName: deployModelName);

var chatPlatformClient = new ChatPlatformClient(client);
await chatPlatformClient.RegisterAgentAsync(pwshDeveloper, agentInfo.SelfDescription);

lifetimeManager.ApplicationStopping.Register(async () =>
{
    Console.WriteLine("Unsubscribing from the agent info stream...");
    await chatPlatformClient.UnregisterAgentAsync(pwshDeveloper);
});

await host.WaitForShutdownAsync();
