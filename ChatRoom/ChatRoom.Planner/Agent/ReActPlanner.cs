using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoGen.Core;

namespace ChatRoom.Planner;

internal class ReActPlanner: IAgent
{
    private readonly IAgent _agent;
    public const string PlannerPrompt = @"Answer the question from user as best you can.

Use the following format:

Question: <the original input question>
Thought: <you should always think about what to do>
Action@<name>: <create an action for <name> to do>

Once you collect enough information from observation, you can provide a final answer to the user.
Thought: I now know the final answer.
Final Answer: <the final answer to original input question>

Begin!";

    public ReActPlanner(IAgent agent)
    {
        _agent = agent;
    }

    public string Name => _agent.Name;

    public Task<IMessage> GenerateReplyAsync(IEnumerable<IMessage> messages, GenerateReplyOptions? options = null, CancellationToken cancellationToken = default)
    {
        return _agent.GenerateReplyAsync(messages, options, cancellationToken);
    }
}
