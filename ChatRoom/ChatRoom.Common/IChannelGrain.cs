using ChatRoom.Common;
using Orleans.Runtime;

namespace ChatRoom;

[Alias("IChannelGrain")]
public interface IChannelGrain : IOrchestratorGrain, IGrainWithStringKey
{
    Task JoinChannel(string name, string description, bool isHuman, IChannelObserver callBack);
    
    Task LeaveChannel(string name);

    internal Task<bool> Message(ChatMsg msg);

    internal Task<ChatMsg[]> ReadHistory(int numberOfMessages);

    internal Task<AgentInfo[]> GetMembers();
}
