using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using Azure.AI.OpenAI;

namespace ChatRoom.OpenAI;

public class OpenAIAgentFactory
{
    private readonly OpenAIAgentConfiguration _config;
    public OpenAIAgentFactory(OpenAIAgentConfiguration config)
    {
        _config = config;
    }

    public IAgent CreateAgent()
    {
        IAgent? agent = default;
        OpenAIClient? openaiClient = default;
        string? deployModelName = default;
        bool useAzure = _config.UseAOAI;
        if (_config.UseAOAI)
        {
            if (_config.AzureOpenAIDeployName is string
            && _config.AzureOpenAIKey is string
            && _config.AzureOpenAIEndpoint is string)
            {
                openaiClient = new OpenAIClient(new Uri(_config.AzureOpenAIEndpoint), new Azure.AzureKeyCredential(_config.AzureOpenAIKey));
                deployModelName = _config.AzureOpenAIDeployName;
            }
            else
            {
                var defaultReply = "Please provide either (AzureOpenAIEndpoint, AzureOpenAIKey, AzureOpenAIDeployName)";

                agent = new DefaultReplyAgent(_config.Name, defaultReply);
            }
        }
        else
        {
            if (_config.OpenAIApiKey is string && _config.OpenAIModelId is string)
            {
                openaiClient = new OpenAIClient(_config.OpenAIApiKey);
                deployModelName = _config.OpenAIModelId;
            }
            else
            {
                var defaultReply = "Please provide either (OpenAIApiKey, OpenAIModelId)";
                agent = new DefaultReplyAgent(_config.Name, defaultReply);
            }
        }

        if (agent is not DefaultReplyAgent && openaiClient is not null && deployModelName is not null)
        {
            agent = new OpenAIChatAgent(
            openAIClient: openaiClient,
            name: _config.Name,
            modelName: deployModelName,
            systemMessage: _config.SystemMessage)
            .RegisterMessageConnector();
        }
        else
        {
            var defaultReply = "Please provide either (AzureOpenAIEndpoint, AzureOpenAIKey, AzureOpenAIDeployName) or (OpenAIApiKey, OpenAIModelId)";
            agent = new DefaultReplyAgent(_config.Name, defaultReply);
        }

        return agent;
    }
}
