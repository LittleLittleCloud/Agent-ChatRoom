namespace ChatRoom.SDK;

public interface IOrchestrator
{
    Task<string?> GetNextSpeaker(AgentInfo[] members, ChatMsg[] messages);
}
