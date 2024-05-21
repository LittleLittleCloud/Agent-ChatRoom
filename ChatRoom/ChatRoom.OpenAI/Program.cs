// See https://aka.ms/new-console-template for more information
using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using Azure.AI.OpenAI;
using ChatRoom.SDK;
using Microsoft.Extensions.Hosting;

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

var agent = new OpenAIChatAgent(
    openAIClient: openaiClient,
    name: "gpt",
    modelName: deployModelName,
    systemMessage: "You are a helpful AI assistant")
    .RegisterMessageConnector();

var host = Host.CreateDefaultBuilder(args)
    .UseChatRoom()
    .Build();

await host.StartAsync();
await host.JoinRoomAsync(agent);
await host.WaitForShutdownAsync();
