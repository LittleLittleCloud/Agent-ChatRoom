using AutoGen.Core;
using ChatRoom.SDK;

namespace ChatRoom.SDK;

internal class DynamicGroupChat : IOrchestrator
{
    private readonly OpenAIClientConfiguration _config;
    public DynamicGroupChat(OpenAIClientConfiguration llmConfig)
    {
        _config = llmConfig;
    }

    public async Task<string?> GetNextSpeaker(AgentInfo[] members, ChatMsg[] messages)
    {
        if (messages is { Length: 0 })
        {
            return null;
        }

        // dumb implementation by using AutoGen GroupChat
        var agents = members.Select(x => new DummyAgent(x)).ToArray();
        // create agents
        var openaiClient = _config.ToOpenAIClient();
        var deployModelName = _config.ModelId;

        if (openaiClient is null || deployModelName is null)
        {
            throw new ArgumentException("channel is not configured properly. Please check the configuration file.");
        }

        var chatHistory = messages.Select(x => x.ToAutoGenMessage()).ToArray();

        IAgent admin = GroupChatAdminFactory.CreateGroupChatAdmin(openaiClient, modelName: deployModelName);
        var groupChat = new GroupChat(
            members: agents,
            admin: admin);

        // add initial messages
        foreach (var agent in members)
        {
            groupChat.AddInitializeMessage(new TextMessage(Role.Assistant, agent.SelfDescription!, agent.Name));
        }

        var nextMessage = await groupChat.CallAsync(chatHistory, maxRound: 1);
        var lastMessage = nextMessage.Last();
        var nextSpeaker = members.First(x => x.Name == lastMessage.From);

        return nextSpeaker.Name;
    }
}
