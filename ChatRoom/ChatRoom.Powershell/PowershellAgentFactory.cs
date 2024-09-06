using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using ChatRoom.SDK.Extension;
using OpenAI;

namespace ChatRoom.Powershell;

internal static class PowershellAgentFactory
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
            chatClient: client.GetChatClient(modelName),
            name: config.Name,
            systemMessage: config.SystemMessage)
            .RegisterMessageConnector()
            .RegisterPrintMessage()
            .ReturnErrorMessageWhenExceptionThrown();

        return agent;
    }
}
