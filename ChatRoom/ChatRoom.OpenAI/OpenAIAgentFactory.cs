using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using ChatRoom.SDK.Extension;
using OpenAI;
namespace ChatRoom.OpenAI;

internal class OpenAIAgentFactory
{
    private readonly OpenAIAgentConfiguration _config;

    public OpenAIAgentFactory(OpenAIAgentConfiguration config)
    {
        _config = config;
    }

    public IAgent CreateAgent()
    {
        IAgent? agent = default;
        OpenAIClient? openaiClient = _config.LLMConfiguration.ToOpenAIClient();
        string? deployModelName = _config.LLMConfiguration.ModelId;

        if (openaiClient is not null && deployModelName is not null)
        {
            agent = new OpenAIChatAgent(
                chatClient: openaiClient.GetChatClient(deployModelName),
                name: _config.Name,
                systemMessage: _config.SystemMessage)
                .RegisterMessageConnector()
                .ReturnErrorMessageWhenExceptionThrown();
        }
        else
        {
            var defaultReply = $"{_config.Name} is not configured properly. Please check the configuration file.";
            agent = new DefaultReplyAgent(_config.Name, defaultReply);
        }

        return agent;
    }

    public static IAgent CreateGroupChatAdmin(OpenAIClient client, string name = "group chat admin", string modelName = "gpt-35-turbo-0125")
    {
        var agent = new OpenAIChatAgent(
            chatClient: client.GetChatClient(modelName),
            name: name)
            .RegisterMessageConnector();

        return agent;
    }
}
