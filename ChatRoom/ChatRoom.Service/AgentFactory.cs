using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using Azure.AI.OpenAI;

namespace ChatRoom.Room;

internal static class AgentFactory
{
    public static IAgent CreateGroupChatAdmin(OpenAIClient client, string name = "group chat admin", string modelName = "gpt-35-turbo-0125")
    {
        var agent = new OpenAIChatAgent(
            openAIClient: client,
            modelName: modelName,
            name: name)
            .RegisterMessageConnector();

        return agent;
    }
}
