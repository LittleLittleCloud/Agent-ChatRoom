using AutoGen.Core;
using Orleans.Streams;

namespace ChatRoom.Common;

public class OrchestratorStreamObserver : IAsyncObserver<AgentInfo>
{
    private readonly IAgent _agent;
    private readonly IChannelGrain _channel;

    public OrchestratorStreamObserver(IAgent agent, IChannelGrain channel)
    {
        _agent = agent;
        _channel = channel;
    }

    public Task OnCompletedAsync() => Task.CompletedTask;

    public Task OnErrorAsync(Exception ex)
    {
        Console.WriteLine(ex.Message);

        return Task.CompletedTask;
    }

    public async Task OnNextAsync(AgentInfo item, StreamSequenceToken? token = null)
    {
        if (item.Name != _agent.Name)
        {
            return;
        }

        var messages = await _channel.ReadHistory(10);

        // convert ChatMsg to TextMessage
        var textMessages = messages.Select(msg => new TextMessage(Role.Assistant, msg.Text, from: msg.From)).ToArray();
        var reply = await _agent!.GenerateReplyAsync(textMessages);

        if (reply.GetContent() is string content)
        {
            var returnMessage = new ChatMsg(_agent.Name, content);
            await _channel.Message(returnMessage);

            return;
        }

        throw new InvalidOperationException("Invalid reply content.");
    }
}
