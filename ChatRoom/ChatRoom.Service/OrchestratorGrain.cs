namespace ChatRoom.Room;

public class OrchestratorGrain : Grain, IOrchestratorGrain
{
    public Task<AgentInfo?> GetNextAgentSpeaker() => throw new NotImplementedException();
}
