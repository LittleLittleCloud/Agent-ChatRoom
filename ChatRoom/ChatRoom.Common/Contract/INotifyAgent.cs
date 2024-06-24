using AutoGen.Core;

namespace ChatRoom.SDK;

public interface INotifyAgent : IAgent
{
    event EventHandler<ChatMsg>? Notify;
}
