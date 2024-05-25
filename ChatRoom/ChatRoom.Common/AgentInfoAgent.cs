using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoGen.Core;

namespace ChatRoom.SDK
{
    internal class AgentInfoAgent
    {
        public AgentInfoAgent(IAgent agent, AgentInfo info)
        {
            Agent = agent;
            Info = info;
        }

        public IAgent Agent { get; }

        public AgentInfo Info { get; }
    }
}
