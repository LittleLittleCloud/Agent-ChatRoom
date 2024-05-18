using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatRoom.Channel;

public class OrchestratorGrain : Grain, IOrchestratorGrain
{
    public Task<AgentInfo?> GetNextAgentSpeaker() => throw new NotImplementedException();
}
