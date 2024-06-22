using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoGen.Core;

namespace ChatRoom.SDK.Extension;

internal static class AgentExtension
{
    public static MiddlewareAgent<TAgent> ReturnErrorMessageWhenExceptionThrown<TAgent>(this TAgent agent)
        where TAgent : IAgent
    {
        return agent.RegisterMiddleware(async (messages, options, next, ct) =>
        {
            try
            {
                return await next.GenerateReplyAsync(messages, options, ct);

            }
            catch (Exception ex)
            {
                return new TextMessage(
                    role: Role.Assistant,
                    @$"An error occurred while processing the message
{ex.Message}
{ex.StackTrace}",
                    from: next.Name);
            }
        });
    }
}
