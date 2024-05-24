using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using Azure.AI.OpenAI;
using ChatRoom.SDK;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;
namespace ChatRoom.OpenAI;

internal class OpenAICommand : AsyncCommand<OpenAICommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, OpenAICommandSettings setting)
    {
        IAgent? agent = default;
        OpenAIClient? openaiClient = default;
        string? deployModelName = default;
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
                var defaultReply = "Please provide either (AzureOpenAIEndpoint, AzureOpenAIKey, AzureOpenAIDeployName)";

                agent = new DefaultReplyAgent(setting.Name, defaultReply);
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
                var defaultReply = "Please provide either (OpenAIApiKey, OpenAIModelId)";
                agent = new DefaultReplyAgent(setting.Name, defaultReply);
            }
        }

        if (agent is not DefaultReplyAgent && openaiClient is not null && deployModelName is not null)
        {
            agent = new OpenAIChatAgent(
            openAIClient: openaiClient,
            name: setting.Name,
            modelName: deployModelName,
            systemMessage: setting.SystemMessage)
            .RegisterMessageConnector();
        }
        else
        {
            var defaultReply = "Please provide either (AzureOpenAIEndpoint, AzureOpenAIKey, AzureOpenAIDeployName) or (OpenAIApiKey, OpenAIModelId)";
            agent = new DefaultReplyAgent(setting.Name, defaultReply);
        }

        var host = Host.CreateDefaultBuilder()
            .AddAgentAsync(async (_) => agent)
            .UseChatRoom(roomName: setting.Room, port: setting.Port)
            .Build();

        await host.StartAsync();
        await host.WaitForAgentsJoinRoomAsync();
        await host.WaitForShutdownAsync();

        return 0;
    }
}
