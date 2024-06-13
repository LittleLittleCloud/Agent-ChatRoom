using Orleans.Runtime;

namespace ChatRoom.Common;

public interface IRoomGrain : IGrainWithStringKey
{
    Task JoinRoom(string name, string description, bool isHuman, IRoomObserver callBack);

    Task LeaveRoom(string nickname);

    Task<AgentInfo[]> GetMembers();

    Task<ChannelInfo[]> GetChannels();

    Task CreateChannel(string channelName, string[]? members = null, ChatMsg[]? chatHistory = null);

    Task DeleteChannel(string channelName);

    Task AddAgentToChannel(string channelName, string agentName);

    Task RemoveAgentFromChannel(string channelName, string agentName);
}
