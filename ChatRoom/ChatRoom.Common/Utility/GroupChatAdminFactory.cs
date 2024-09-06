using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using Azure.AI.OpenAI;
using ChatRoom.SDK.Extension;
using OpenAI;
namespace ChatRoom.SDK;

internal class GroupChatAdminFactory
{
    public static IAgent CreateGroupChatAdmin(OpenAIClient client, string name = "admin", string modelName = "gpt-35-turbo-0125")
    {
        var agent = new OpenAIChatAgent(
            chatClient: client.GetChatClient(modelName),
            name: name)
            .RegisterMessageConnector();

        return agent;
    }
}
