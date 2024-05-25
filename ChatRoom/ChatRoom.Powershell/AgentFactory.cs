using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using Azure.AI.OpenAI;

namespace ChatRoom.Powershell;

internal static class AgentFactory
{
    public static IAgent CreatePwshDeveloperAgent(
        PowershellGPTConfiguration config)
    {
        OpenAIClient client;
        string modelName;
        if (config.LLMType == LLMType.AOAI)
        {
            var azureOpenAiEndpoint = config.AzureOpenAiEndpoint;
            var azureOpenAiKey = config.AzureOpenAiKey;
            var azureDeploymentName = config.AzureDeploymentName;

            if (string.IsNullOrWhiteSpace(azureOpenAiEndpoint) || string.IsNullOrWhiteSpace(azureOpenAiKey) || string.IsNullOrWhiteSpace(azureDeploymentName))
            {
                var defaultReply = "Azure OpenAI endpoint, key, or deployment name not found. Please provide Azure OpenAI endpoint, key, and deployment name in the configuration file or via env:AZURE_OPENAI_ENDPOINT, env:AZURE_OPENAI_API_KEY, env:AZURE_OPENAI_DEPLOY_NAME";
                return new DefaultReplyAgent(config.Name, defaultReply);
            }

            client = new OpenAIClient(new Uri(azureOpenAiEndpoint), new Azure.AzureKeyCredential(azureOpenAiKey));
            modelName = azureDeploymentName;
        }
        else if (config.LLMType == LLMType.OpenAI)
        {
            var openaiApiKey = config.OpenAiApiKey;
            var openaiModelId = config.OpenAiModelId;

            if (string.IsNullOrWhiteSpace(openaiApiKey))
            {
                var defaultReply = "OpenAI API key not found. Please provide OpenAI API key in the configuration file or via env:OPENAI_API_KEY";
                return new DefaultReplyAgent(config.Name, defaultReply);
            }

            client = new OpenAIClient(openaiApiKey);
            modelName = openaiModelId;
        }
        else
        {
            var errorMessage = "Please provide either (AzureOpenAiEndpoint, AzureOpenAiKey, AzureDeploymentName) or OpenAiApiKey";
            return new DefaultReplyAgent(config.Name, errorMessage);
        }

        var agent = new OpenAIChatAgent(
            openAIClient: client,
            modelName: modelName,
            name: config.Name,
            systemMessage: config.SystemMessage)
            .RegisterMessageConnector()
            .RegisterPrintMessage();

        return agent;
    }
}
