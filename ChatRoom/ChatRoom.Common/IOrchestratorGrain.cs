namespace ChatRoom;

public interface IOrchestratorGrain : IGrainWithStringKey
{
    Task<AgentInfo?> GetNextAgentSpeaker();
}
