using Microsoft.Extensions.Logging;

namespace ChatRoom.SDK;

internal class RoundRobin : IOrchestrator
{
    public readonly ILogger<RoundRobin>? _logger;

    public RoundRobin(
        ILogger<RoundRobin>? logger = null)
    {
        _logger = logger;
    }

    public async Task<string?> GetNextSpeaker(AgentInfo[] members, ChatMsg[] messages)
    {
        _logger?.LogInformation("RoundRobinOrchestrator.GetNextSpeaker");
        var lastSpeaker = members.LastOrDefault(m => m.Name == messages.LastOrDefault()?.From);

        if (lastSpeaker == null)
        {
            _logger?.LogInformation("No last speaker found");
            return null;
        }
        else
        {
            _logger?.LogInformation($"Last speaker: {lastSpeaker.Name}");
            var index = Array.IndexOf(members, lastSpeaker);
            return members[(index + 1) % members.Length].Name;
        }
    }
}
