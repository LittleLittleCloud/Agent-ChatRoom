using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using Azure.AI.OpenAI;
using ChatRoom.SDK.Extension;

namespace ChatRoom.Powershell;

internal static class AgentFactory
{
    public static IAgent CreatePwshDeveloperAgent(
        PowershellGPTConfiguration config)
    {
        OpenAIClient? client = config.OpenAIConfiguration?.ToOpenAIClient();
        string? modelName = config.OpenAIConfiguration?.ModelId;
        
        if (client is null || modelName is null)
        {
            return new DefaultReplyAgent(
                config.Name,
                $"{config.Name} is not configured properly. Please check the configuration file.");
        }

        var agent = new OpenAIChatAgent(
            openAIClient: client,
            modelName: modelName,
            name: config.Name,
            systemMessage: config.SystemMessage)
            .RegisterMessageConnector()
            .RegisterPrintMessage()
            .ReturnErrorMessageWhenExceptionThrown();

        return agent;
    }
}
