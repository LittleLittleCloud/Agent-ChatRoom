using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatRoom.SDK;
using Microsoft.Extensions.Logging;

namespace ChatRoom.Client;

internal class ManualOrchestartor : IOrchestrator
{
    private readonly ILogger<ManualOrchestartor>? _logger;

    public ManualOrchestartor(
        ILogger<ManualOrchestartor>? logger = null)
    {
        _logger = logger;
    }
    public async Task<string?> GetNextSpeaker(AgentInfo[] members, ChatMsg[] messages)
    {
        _logger?.LogInformation("ManualOrchestartor.GetNextSpeaker");
        return null;
    }
}
