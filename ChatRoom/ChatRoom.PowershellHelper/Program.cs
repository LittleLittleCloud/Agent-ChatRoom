// See https://aka.ms/new-console-template for more information
using Azure.AI.OpenAI;
using ChatRoom;
using ChatRoom.PowershellHelper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using var host = new HostBuilder()
    .UseOrleansClient(clientBuilder =>
    {
        clientBuilder
            .UseLocalhostClustering();
    })
    .Build();

var client = host.Services.GetRequiredService<IClusterClient>();

await host.StartAsync();
Console.WriteLine("Client is connected to the server.");

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
var manager = AgentFactory.CreateManagerAgent(openaiClient, modelName: deployModelName);
var pwshDeveloper = AgentFactory.CreatePwshDeveloperAgent(openaiClient, Environment.CurrentDirectory, modelName: deployModelName);

// join the General channel
var roomName = "General";
var channel = client.GetGrain<IChannelGrain>(roomName);
var streamId = await channel.Join("PowershellHelper");
Console.WriteLine("Joined the General channel.");

await Host.CreateDefaultBuilder(args)
    .UseOrleans(siloBuilder =>
    {
        siloBuilder
            .UseLocalhostClustering(siloPort: 12345, gatewayPort: 34567)
            .AddMemoryGrainStorage("PubSubStore");
    })
    .RunConsoleAsync();

