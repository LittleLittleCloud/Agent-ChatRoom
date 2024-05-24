// See https://aka.ms/new-console-template for more information
using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using Azure.AI.OpenAI;
using ChatRoom.OpenAI;
using ChatRoom.SDK;
using Microsoft.Extensions.Hosting;
var host = Host.CreateDefaultBuilder(args)
    .AddAgentAsync<OpenAIConfiguration, IAgent>(async (_, setting) =>
    {
        IAgent agent;
        OpenAIClient openaiClient;
        string deployModelName;
        bool useAzure = setting.UseAOAI;
        if (setting.UseAOAI)
        {
            if (setting.AzureOpenAIDeployName is string
            && setting.AzureOpenAIKey is string
            && setting.AzureOpenAIEndpoint is string)
            {
                openaiClient = new OpenAIClient(new Uri(setting.AzureOpenAIEndpoint), new Azure.AzureKeyCredential(setting.AzureOpenAIKey));
                deployModelName = setting.AzureOpenAIDeployName;
            }
            else
            {
                throw new ArgumentException("Please provide either (AzureOpenAIEndpoint, AzureOpenAIKey, AzureOpenAIDeployName)");
            }
        }
        else
        {
            if (setting.OpenAIApiKey is string && setting.OpenAIModelId is string)
            {
                openaiClient = new OpenAIClient(setting.OpenAIApiKey);
                deployModelName = setting.OpenAIModelId;
            }
            else
            {
                throw new ArgumentException("Please provide OpenAIApiKey");
            }
        }

        agent = new OpenAIChatAgent(
            openAIClient: openaiClient,
            name: setting.Name,
            modelName: deployModelName,
            systemMessage: setting.SystemMessage)
            .RegisterMessageConnector();

        return agent;
    })
    .UseChatRoom()
    .Build();

await host.StartAsync();
await host.WaitForAgentsJoinRoomAsync();
await host.WaitForShutdownAsync();
