using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatRoom.SDK;
using Microsoft.Extensions.Logging;

namespace ChatRoom.Client;

internal class RoundRobinOrchestrator : IOrchestrator
{
    public readonly ILogger<RoundRobinOrchestrator>? _logger;

    public RoundRobinOrchestrator(
        ILogger<RoundRobinOrchestrator>? logger = null)
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
