using Orleans.Concurrency;

namespace ChatRoom.Common;

public interface IChannelObserver : IOrchestratorObserver
{
    [OneWay]
    Task Join(AgentInfo agent, ChannelInfo channelInfo);

    [OneWay]
    Task Leave(AgentInfo agent, ChannelInfo channelInfo);

    Task<bool> Ping();

    Task<ChatMsg?> GenerateReplyAsync(AgentInfo agent, ChatMsg[] msg);

    [OneWay]
    Task NewMessage(ChatMsg msg);
}
