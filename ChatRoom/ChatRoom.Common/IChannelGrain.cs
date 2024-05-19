using ChatRoom.Common;
using Orleans.Runtime;

namespace ChatRoom;

[Alias("IChannelGrain")]
public interface IChannelGrain : IOrchestratorGrain, IGrainWithStringKey
{
    Task Join(AgentInfo nickname);
    Task Leave(AgentInfo nickname);

    Task Subscribe(IChannelObserver observer);

    Task Unsubscribe(IChannelObserver observer);

    internal Task<bool> Message(ChatMsg msg);
    internal Task<ChatMsg[]> ReadHistory(int numberOfMessages);
    internal Task<AgentInfo[]> GetMembers();
}
