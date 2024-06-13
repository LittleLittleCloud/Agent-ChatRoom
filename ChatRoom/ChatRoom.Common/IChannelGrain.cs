using ChatRoom.Common;
using Orleans.Runtime;

namespace ChatRoom;

[Alias("IChannelGrain")]
public interface IChannelGrain : IOrchestratorGrain, IGrainWithStringKey
{
    Task JoinChannel(string name, string description, bool isHuman, IChannelObserver callBack);
    
    Task LeaveChannel(string name);

    Task SendNotification(ChatMsg msg);

    Task ClearHistory();

    Task<ChannelInfo> GetChannelInfo();

    internal Task InitializeChatHistory(ChatMsg[] history);

    internal Task<bool> SendMessage(ChatMsg msg);

    internal Task DeleteMessage(long msgId);

    internal Task EditTextMessage(long msgId, string newText);

    internal Task<ChatMsg[]> ReadHistory(int numberOfMessages);

    internal Task<AgentInfo[]> GetMembers();
}
