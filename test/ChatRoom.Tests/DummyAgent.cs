using AutoGen.Core;

namespace ChatRoom.Tests;

internal class DummyAgent : IAgent
{
    private readonly AgentInfo _agentInfo;

    public DummyAgent(AgentInfo agentInfo)
    {
        _agentInfo = agentInfo;
    }

    public string Name => _agentInfo.Name;

    public AgentInfo AgentInfo => _agentInfo;

    public async Task<IMessage> GenerateReplyAsync(IEnumerable<IMessage> messages, GenerateReplyOptions? options = null, CancellationToken cancellationToken = default)
    {
        return new ChatMsg(
            _agentInfo.Name,
            "I'm a dummy agent, I don't know how to reply to messages.");
    }
}
