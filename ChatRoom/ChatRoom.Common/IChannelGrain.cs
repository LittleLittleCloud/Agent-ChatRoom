using Orleans.Runtime;

namespace ChatRoom;

public interface IChannelGrain : IOrchestratorGrain, IGrainWithStringKey
{
    Task<StreamId> Join(AgentInfo nickname);
    Task<StreamId> Leave(AgentInfo nickname);
    Task<bool> Message(ChatMsg msg);
    Task<ChatMsg[]> ReadHistory(int numberOfMessages);
    Task<AgentInfo[]> GetMembers();
}
