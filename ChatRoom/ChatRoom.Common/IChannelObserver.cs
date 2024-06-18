using Orleans.Concurrency;

namespace ChatRoom.SDK;

public interface IChannelObserver : INotificationObserver
{
    [OneWay]
    Task JoinChannel(AgentInfo agent, ChannelInfo channelInfo);

    [OneWay]
    Task LeaveChannel(AgentInfo agent, ChannelInfo channelInfo);

    Task<bool> Ping();

    Task<ChatMsg?> GenerateReplyAsync(AgentInfo agent, ChatMsg[] msg, ChannelInfo channelInfo);

    [OneWay]
    Task NewMessage(ChatMsg msg);
}
