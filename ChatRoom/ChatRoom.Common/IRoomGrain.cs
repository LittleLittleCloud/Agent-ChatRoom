using Orleans.Runtime;

namespace ChatRoom.SDK;

public interface IRoomGrain : IGrainWithStringKey
{
    Task AddAgentToRoom(string name, string description, bool isHuman, IRoomObserver callBack);

    Task RemoveAgentFromRoom(string name);

    Task<AgentInfo[]> GetMembers();

    Task<string[]> GetOrchestrators();

    Task<ChannelInfo[]> GetChannels();

    Task CreateChannel(string channelName, string[]? members = null, ChatMsg[]? chatHistory = null, string[]? orchestrators = null);

    Task CloneChannel(string oldChannelName, string newChannelName);

    Task DeleteChannel(string channelName);

    Task AddAgentToChannel(string channelName, string agentName);

    Task RemoveAgentFromChannel(string channelName, string agentName);

    Task AddOrchestratorToRoom(string name, IOrchestratorObserver orchestrator);

    Task RemoveOrchestratorFromRoom(string name);

    Task AddOrchestratorToChannel(string channelName, string orchestratorName);

    Task RemoveOrchestratorFromChannel(string channelName, string orchestratorName);
}
