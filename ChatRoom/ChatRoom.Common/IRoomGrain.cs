﻿using Orleans.Runtime;

namespace ChatRoom.Common;

public interface IRoomGrain : IGrainWithStringKey
{
    Task<StreamId> Join(AgentInfo nickname);
    Task<StreamId> Leave(string nickname);
    Task<AgentInfo[]> GetMembers();
    Task<ChannelInfo[]> GetChannels();
    Task CreateChannel(ChannelInfo channelInfo);
    Task DeleteChannel(string channelName);
    Task AddAgentToChannel(ChannelInfo channelInfo, AgentInfo agentInfo);
    Task RemoveAgentFromChannel(ChannelInfo channelInfo, AgentInfo agentInfo);

    Task Subscribe(IRoomObserver observer);

    Task Unsubscribe(IRoomObserver observer);
}