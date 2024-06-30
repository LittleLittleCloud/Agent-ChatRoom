namespace ChatRoom.SDK;

internal interface IOrchestratorObserver : IOrchestrator, IGrainObserver
{
}

internal class OrchestratorObserver : IOrchestratorObserver
{
    private readonly IOrchestrator _orchestrator;

    public OrchestratorObserver(IOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    public Task<string?> GetNextSpeaker(AgentInfo[] members, ChatMsg[] messages)
    {
        return _orchestrator.GetNextSpeaker(members, messages);
    }
}
