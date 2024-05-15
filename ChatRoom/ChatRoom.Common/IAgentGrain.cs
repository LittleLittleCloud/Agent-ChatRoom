using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoGen.Core;

namespace ChatRoom;

public interface IAgentGrain : IGrainWithStringKey
{
    Task<AgentInfo> GetAgentInfo();

    Task<ChatMsg> GenerateReplyAsync(ChatMsg[] messages);
}
