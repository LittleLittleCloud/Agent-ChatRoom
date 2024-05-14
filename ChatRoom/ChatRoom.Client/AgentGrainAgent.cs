using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoGen.Core;

namespace ChatRoom.Client;

internal class AgentGrainAgent : IAgent
{
    private readonly IAgentGrain _grain;

    public AgentGrainAgent(IAgentGrain grain)
    {
        _grain = grain;
    }

    public string Name => _grain.GetName().Result;

    public async Task<IMessage> GenerateReplyAsync(IEnumerable<IMessage> messages, GenerateReplyOptions? options = null, CancellationToken cancellationToken = default)
    {
        if (messages.Any(m => !(m is ChatMsg)))
        {
            throw new ArgumentException("Only ChatMsg messages are supported.");
        }

        var chatMsgs = messages.Cast<ChatMsg>().ToArray();

        return await _grain.GenerateReplyAsync(chatMsgs);
    }
}
