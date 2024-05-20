using Orleans.Concurrency;

namespace ChatRoom.Common;

public interface INotificationObserver : IGrainObserver
{
    [OneWay]
    Task Notification(ChatMsg msg);
}

public interface IRoomObserver : IChannelObserver, INotificationObserver
{

    [OneWay]
    Task Join(AgentInfo agent, string room);

    [OneWay]
    Task Leave(AgentInfo agent, string room);
}
