﻿using ChatRoom.SDK;

namespace ChatRoom.SDK;

[Alias("IChannelGrain")]
internal interface IChannelGrain : IGrainWithStringKey
{
    Task AddAgentToChannel(string name, string description, bool isHuman, IChatRoomAgentObserver callBack);

    Task RemoveAgentFromChannel(string name);

    Task AddOrchestratorToChannel(string name, IOrchestratorObserver orchestrator);

    Task RemoveOrchestratorFromChannel(string name);

    Task SendNotification(ChatMsg msg);

    Task ClearHistory();

    Task<ChannelInfo> GetChannelInfo();

    internal Task<ChatMsg?> GenerateNextReply(string[]? candidates = null, ChatMsg[]? msgs = null, string? orchestrator = null);

    internal Task InitializeChatHistory(ChatMsg[] history);

    internal Task SendMessage(ChatMsg msg);

    internal Task DeleteMessage(long msgId);

    internal Task EditTextMessage(long msgId, string newText);

    internal Task<ChatMsg[]> ReadHistory(int numberOfMessages);

    internal Task<AgentInfo[]> GetMembers();

    internal Task Delete();
}
