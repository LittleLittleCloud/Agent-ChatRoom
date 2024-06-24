using System;
using AutoGen.Core;
using Azure.AI.OpenAI;
using ChatRoom.SDK;
using ChatRoom.OpenAI;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Orleans.Runtime;
using OrleansCodeGen.Orleans;

namespace ChatRoom.Room;

internal class ChannelGrain : Grain, IChannelGrain
{
    private static readonly object _lock = new();
    private readonly List<ChatMsg> _messages = new(100);
    private readonly ILogger _logger;
    private readonly Dictionary<AgentInfo, IChatRoomAgentObserver> _agents = new();
    private readonly Dictionary<string, IOrchestratorObserver> _orchestrators = new();
    private readonly ChannelConfiguration _config;

    public ChannelGrain(
        ChannelConfiguration config,
        ILogger<ChannelGrain> logger)
    {
        _logger = logger;
        _config = config;
        this.DelayDeactivation(TimeSpan.MaxValue);
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Channel {ChannelId} activated", this.GetPrimaryKeyString());
        
        await base.OnActivateAsync(cancellationToken);
    }

    public async Task Delete()
    {
       _logger.LogInformation("Channel {ChannelId} deactivated", this.GetPrimaryKeyString());
        this.DeactivateOnIdle();
    }

    public async Task AddAgentToChannel(string name, string description, bool isHuman, IChatRoomAgentObserver callBack)
    {
        var agentInfo = new AgentInfo(name, description, isHuman);
        // check if agent is already in _agents
        if (_agents.ContainsKey(agentInfo))
        {
            return;
        }

        _agents[agentInfo] = callBack;

        var channelInfo = await GetChannelInfo();
        _logger.LogInformation("Agent {AgentName} joined channel {ChannelId}", agentInfo.Name, channelInfo);

        foreach (var cb in _agents.Values)
        {
            _logger.LogInformation("Notifying {AgentName} about new agent {NewAgentName}", cb, agentInfo.Name);
            await cb.JoinChannel(agentInfo, channelInfo);
        }
    }

    public async Task RemoveAgentFromChannel(string name)
    {
        var agentInfo = _agents.Keys.FirstOrDefault(x => x.Name == name);
        if (agentInfo is null)
        {
            return;
        }

        _agents.Remove(agentInfo);
        var channelInfo = await GetChannelInfo();
        _logger.LogInformation("Agent {AgentName} left channel {ChannelId}", agentInfo.Name, channelInfo);
        foreach (var cb in _agents.Values)
        {
            _logger.LogInformation("Notifying {AgentName} about agent {LeavingAgentName} leaving", cb, agentInfo.Name);
            await cb.LeaveChannel(agentInfo, channelInfo);
        }
    }

    public async Task SendMessage(ChatMsg msg)
    {
        var channelInfo = await GetChannelInfo();
        _logger.LogInformation("Received message from {From} in channel {ChannelId}", msg.From, channelInfo);
        
        if (msg.From == "System")
        {
            _logger.LogInformation("System message received. Ignoring.");
            return;
        }

        lock (_lock)
        {
            _messages.Add(msg);
        }
        
        foreach (var cb in _agents.Values)
        {
            _logger.LogInformation("Notifying {AgentName} about new message", cb);
            await cb.NewMessage(msg);
        }
    }

    public Task<AgentInfo[]> GetMembers() => Task.FromResult(_agents.Keys.ToArray());

    public Task<ChatMsg[]> ReadHistory(int numberOfMessages)
    {
        lock (_lock)
        {
            var response = _messages
            .OrderByDescending(x => x.Created)
            .Take(numberOfMessages)
            .OrderBy(x => x.Created)
            .ToArray();
            return Task.FromResult(response);
        }
    }

    public Task InitializeChatHistory(ChatMsg[] history)
    {
        this._messages.Clear();
        this._messages.AddRange(history);

        return Task.CompletedTask;
    }

    public Task SendNotification(ChatMsg msg)
    {
        var content = msg.GetContent();
        if (content == null)
        {
            return Task.CompletedTask;
        }

        _logger.LogInformation("Sending notification: {Content}", content);

        foreach (var cb in _agents.Values)
        {
            _ = cb.Notification(msg);
        }

        return Task.CompletedTask;
    }

    public Task ClearHistory()
    {
        _logger.LogInformation("Clearing chat history");
        lock (_lock)
        {
            _messages.Clear();
        }

        return Task.CompletedTask;
    }

    public Task DeleteMessage(long msgId)
    {
        var msg = _messages.FirstOrDefault(x => x.ID == msgId);
        if (msg is not null)
        {
            lock (_lock)
            {
                _messages.Remove(msg);
            }
        }

        return Task.CompletedTask;
    }

    public Task EditTextMessage(long msgId, string newText)
    {
        var msg = _messages.FirstOrDefault(x => x.ID == msgId);
        if (msg is not null && msg.Parts.Length == 1 && msg.Parts[0].TextPart is not null)
        {
            lock (_lock)
            {
                msg.Parts[0].TextPart = newText;
            }
        }

        return Task.CompletedTask;
    }

    public Task<ChannelInfo> GetChannelInfo()
    {
        var channelInfo = new ChannelInfo(this.GetPrimaryKeyString())
        {
            Members = _agents.Keys.ToList(),
            Orchestrators = _orchestrators.Keys.ToList(),
        };
        return Task.FromResult(channelInfo);
    }

    public async Task<ChatMsg?> GenerateNextReply(string[]? candidates = null, ChatMsg[]? msgs = null, string? orchestrator = null)
    {
        candidates = candidates ??= _agents.Keys.Select(x => x.Name).ToArray();
        var agents = _agents.Where(x => candidates.Contains(x.Key.Name)).Select(x => x.Key).ToArray();
        
        if (orchestrator is null || !_orchestrators.ContainsKey(orchestrator))
        {
            _logger?.LogInformation($"Generate null reply because orchestrator {orchestrator} is not found");
            return null;
        }
        
        var orchestratorObserver = _orchestrators[orchestrator];
        msgs ??= _messages.ToArray();

        _logger.LogInformation("Getting next agent speaker");
        var nextSpeakerName = agents.Length == 1 ? agents[0].Name : await orchestratorObserver.GetNextSpeaker(agents, msgs);
        
        var nextSpeaker = agents.FirstOrDefault(x => x.Name == nextSpeakerName);
        if (nextSpeaker is null)
        {
            _logger.LogInformation("No next speaker found");
            return null;
        }

        _logger.LogInformation("Next Speaker: {NextSpeaker}", nextSpeaker.Name);
    
        var nextSpeakerObserver = _agents[nextSpeaker];
        var channelInfo = await GetChannelInfo();
        var reply = await nextSpeakerObserver.GenerateReplyAsync(nextSpeaker, msgs, channelInfo);

        if (reply is not null)
        {
            _logger.LogInformation("Generated reply: {Reply}", reply.GetContent());

            await this.SendMessage(reply);
        }
        return reply;
    }

    public Task AddOrchestratorToChannel(string name, IOrchestratorObserver orchestrator)
    {
        _logger.LogInformation("Adding orchestrator {OrchestratorName} to channel {ChannelId}", name, this.GetPrimaryKeyString());

        if (_orchestrators.ContainsKey(name))
        {
            return Task.CompletedTask;
        }

        _orchestrators[name] = orchestrator;
        return Task.CompletedTask;
    }

    public Task RemoveOrchestratorFromChannel(string name)
    {
        _logger.LogInformation("Removing orchestrator {OrchestratorName} from channel {ChannelId}", name, this.GetPrimaryKeyString());

        if (_orchestrators.ContainsKey(name))
        {
            _orchestrators.Remove(name);
        }

        return Task.CompletedTask;
    }
}
