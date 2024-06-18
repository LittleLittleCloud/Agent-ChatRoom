using Orleans.Concurrency;

namespace ChatRoom.SDK;

public interface INotificationObserver : IGrainObserver
{
    [OneWay]
    Task Notification(ChatMsg msg);
}

public interface IRoomObserver : IChannelObserver, INotificationObserver
{
    [OneWay]
    Task JoinRoom(AgentInfo agent, string room);

    [OneWay]
    Task LeaveRoom(AgentInfo agent, string room);
}
