using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoGen.Core;

namespace ChatRoom.SDK.Orleans;

internal class AutoGenOrchestratorObserver : IOrchestratorObserver
{
    private readonly AutoGen.Core.IOrchestrator _orchestrator;
    public AutoGenOrchestratorObserver(AutoGen.Core.IOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    public async Task<string?> GetNextSpeaker(AgentInfo[] members, ChatMsg[] messages)
    {
        var autogenMessages = messages.Select(msg => msg.ToAutoGenMessage()).ToArray();
        var dummyAgents = members.Select(x => new DummyAgent(x)).ToArray();

        var context = new OrchestrationContext()
        {
            Candidates = dummyAgents,
            ChatHistory = autogenMessages,
        };

        var nextAgent = await _orchestrator.GetNextSpeakerAsync(context);

        return nextAgent?.Name;
    }
}
