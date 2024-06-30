using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatRoom.SDK;

namespace ChatRoom.SDK;

public record class SendTextMessageToChannelRequest
{
    public SendTextMessageToChannelRequest(string channelName, ChatMsg message)
    {
        ChannelName = channelName;
        Message = message;
    }

    public string ChannelName { get; init; }

    public ChatMsg Message { get; init; }
}

public record class GetChannelMembersRequest
{
    public GetChannelMembersRequest(string channelName)
    {
        ChannelName = channelName;
    }

    public string ChannelName { get; init; }
}

public record class GetChannelChatHistoryRequest
{
    public GetChannelChatHistoryRequest(string channelName, int count)
    {
        ChannelName = channelName;
        Count = count;
    }

    public string ChannelName { get; init; }

    public int Count { get; init; }
}

public record class JoinChannelRequest
{    
    public JoinChannelRequest(string channelName, bool createIfNotExists)
    {
        ChannelName = channelName;
        CreateIfNotExists = createIfNotExists;
    }

    public string ChannelName { get; init; }

    public bool CreateIfNotExists { get; init; }
}

public record class LeaveChannelRequest
{
    public LeaveChannelRequest(string channelName)
    {
        ChannelName = channelName;
    }

    public string ChannelName { get; init; }
}

public record class CreateChannelRequest
{
    public CreateChannelRequest(string channelName)
    {
        ChannelName = channelName;
    }

    public string ChannelName { get; init; }
}

public record class DeleteChannelRequest
{
    public DeleteChannelRequest(string channelName)
    {
        ChannelName = channelName;
    }

    public string ChannelName { get; init; }
}

public record class AddAgentToChannelRequest
{
    public AddAgentToChannelRequest(string channelName, string agentName)
    {
        ChannelName = channelName;
        AgentName = agentName;
    }

    public string ChannelName { get; init; }

    public string AgentName { get; init; }
}

public record class RemoveAgentFromChannelRequest
{
    public RemoveAgentFromChannelRequest(string channelName, string agentName)
    {
        ChannelName = channelName;
        AgentName = agentName;
    }

    public string ChannelName { get; init; }

    public string AgentName { get; init; }
}

public record class EditTextMessageRequest
{
    public EditTextMessageRequest(string channelName, long messageId, string newText)
    {
        ChannelName = channelName;
        MessageId = messageId;
        NewText = newText;
    }

    public string ChannelName { get; init; }

    public long MessageId { get; init; }

    public string NewText { get; init; }
}

public record class GenerateNextReplyRequest
{
    public GenerateNextReplyRequest(
        string channelName,
        ChatMsg[] chatMsgs, 
        string[] candidates,
        string? orchestrator = null)
    {
        ChannelName = channelName;
        ChatMsgs = chatMsgs;
        Candidates = candidates;
        Orchestrator = orchestrator;
    }

    public string ChannelName { get; init; }

    public ChatMsg[] ChatMsgs { get; init; }

    public string[] Candidates { get; init; }

    public string? Orchestrator { get; init; }
}

public record class GenerateNextReplyResponse
{
    public GenerateNextReplyResponse(ChatMsg? message)
    {
        Message = message;
    }

    public ChatMsg? Message { get; init; }
}

public record class AddOrchestratorToChannelRequest
{
    public AddOrchestratorToChannelRequest(string channelName, string orchestratorName)
    {
        ChannelName = channelName;
        OrchestratorName = orchestratorName;
    }

    public string ChannelName { get; init; }

    public string OrchestratorName { get; init; }
}

public record class RemoveOrchestratorFromChannelRequest
{
    public RemoveOrchestratorFromChannelRequest(string channelName, string orchestratorName)
    {
        ChannelName = channelName;
        OrchestratorName = orchestratorName;
    }

    public string ChannelName { get; init; }

    public string OrchestratorName { get; init; }
}

public record class CloneChannelRequest
{
    public CloneChannelRequest(string channelName, string newChannelName)
    {
        ChannelName = channelName;
        NewChannelName = newChannelName;
    }

    public string ChannelName { get; init; }

    public string NewChannelName { get; init; }
}

public record class EditChannelNameRequest
{
    public EditChannelNameRequest(string oldChannelName, string newChannelName)
    {
        OldChannelName = oldChannelName;
        NewChannelName = newChannelName;
    }

    public string OldChannelName { get; init; }

    public string NewChannelName { get; init; }
}
