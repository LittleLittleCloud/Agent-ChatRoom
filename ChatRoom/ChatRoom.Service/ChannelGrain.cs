using System;
using AutoGen.Core;
using Azure.AI.OpenAI;
using ChatRoom.Common;
using ChatRoom.OpenAI;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Orleans.Runtime;

namespace ChatRoom.Room;

internal class ChannelGrain : Grain, IChannelGrain
{
    private static readonly object _lock = new();
    private readonly List<ChatMsg> _messages = new(100);
    private ChannelInfo _channelInfo = null!;
    private readonly ILogger _logger;
    private readonly Dictionary<AgentInfo, IChannelObserver> _agents = new();
    private readonly ChannelConfiguration _config;

    public ChannelGrain(
        ChannelConfiguration config,
        ILogger<ChannelGrain> logger)
    {
        _logger = logger;
        _config = config;
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _channelInfo = new ChannelInfo(this.GetPrimaryKeyString());
        _logger.LogInformation("Channel {ChannelId} activated", _channelInfo.Name);
        
        await base.OnActivateAsync(cancellationToken);
    }

    public async Task JoinChannel(string name, string description, bool isHuman, IChannelObserver callBack)
    {
        var agentInfo = new AgentInfo(name, description, isHuman);
        // check if agent is already in _agents
        if (_agents.ContainsKey(agentInfo))
        {
            return;
        }

        _agents[agentInfo] = callBack;

        _logger.LogInformation("Agent {AgentName} joined channel {ChannelId}", agentInfo.Name, _channelInfo.Name);

        foreach (var cb in _agents.Values)
        {
            _logger.LogInformation("Notifying {AgentName} about new agent {NewAgentName}", cb, agentInfo.Name);
            await cb.JoinChannel(agentInfo, _channelInfo);
        }
    }

    public async Task LeaveChannel(string name)
    {
        var agentInfo = _agents.Keys.FirstOrDefault(x => x.Name == name);
        if (agentInfo is null)
        {
            return;
        }

        _agents.Remove(agentInfo);
        _logger.LogInformation("Agent {AgentName} left channel {ChannelId}", agentInfo.Name, _channelInfo.Name);

        foreach (var cb in _agents.Values)
        {
            _logger.LogInformation("Notifying {AgentName} about agent {LeavingAgentName} leaving", cb, agentInfo.Name);
            await cb.LeaveChannel(agentInfo, _channelInfo);
        }
    }

    public async Task SendMessage(ChatMsg msg)
    {
        _logger.LogInformation("Received message from {From} in channel {ChannelId}", msg.From, _channelInfo.Name);
        
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

    private async Task<AgentInfo?> GetNextAgentSpeaker(AgentInfo[] members, ChatMsg[] messages)
    {
        var humanMembers = members.Where(x => x.IsHuman).ToArray();
        var notHumanMembers = members.Where(x => !x.IsHuman).ToArray();
        var humanAgents = humanMembers.Select(x => new DummyAgent(x)).ToArray();
        var notHumanAgents = notHumanMembers.Select(x => new DummyAgent(x)).ToArray();
        var agents = humanAgents.Concat(notHumanAgents).ToArray();
        // create agents
        var openaiClient = _config.OpenAIConfiguration?.ToOpenAIClient();
        var deployModelName = _config.OpenAIConfiguration?.ModelId;

        if (openaiClient is null || deployModelName is null)
        {
            throw new ArgumentException("channel is not configured properly. Please check the configuration file.");
        }
        

        // create graph chat
        // allow not human agents <-> human agents 
        var transitions = new List<Transition>();
        foreach (var humanAgent in humanAgents)
        {
            foreach (var notHumanAgent in notHumanAgents)
            {
                transitions.Add(Transition.Create(notHumanAgent, humanAgent));
                transitions.Add(Transition.Create(humanAgent, notHumanAgent));
            }
        }

        var graph = new Graph(transitions);
        var chatHistory = messages.Select(x => x.ToAutoGenMessage()).ToArray();
        var lastSpeaker = agents.First(x => x.Name == messages.Last().From);
        var nextAvailableAgents = await graph.TransitToNextAvailableAgentsAsync(lastSpeaker, _messages);
        if (nextAvailableAgents.Count() == 1 && members.Any(x => x.Name == nextAvailableAgents.First().Name))
        {
            var nextSpeaker = members.First(x => x.Name == nextAvailableAgents.First().Name);
            return nextSpeaker;
        }
        else if (nextAvailableAgents.Count() > 1)
        {
            IAgent admin = AgentFactory.CreateGroupChatAdmin(openaiClient, modelName: deployModelName);
            var groupChat = new GroupChat(
                workflow: graph,
                members: humanAgents.Concat(notHumanAgents),
                admin: admin);

            // add initial messages
            foreach (var agent in humanMembers.Concat(notHumanMembers))
            {
                groupChat.AddInitializeMessage(new TextMessage(Role.Assistant, agent.SelfDescription!, agent.Name));
            }

            var nextMessage = await groupChat.CallAsync(chatHistory, maxRound: 1);
            var lastMessage = nextMessage.Last();
            var nextSpeaker = members.First(x => x.Name == lastMessage.From);
            if (nextAvailableAgents.Any(x => x.Name == nextSpeaker.Name))
            {
                return nextSpeaker;
            }

            return null;
        }
        else
        {
            return null;
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
            Members = _agents.Keys.ToList()
        };
        return Task.FromResult(channelInfo);
    }

    public async Task<ChatMsg?> GenerateNextReply(string[]? candidates = null, ChatMsg[]? msgs = null)
    {
        candidates = candidates ??= _agents.Keys.Select(x => x.Name).ToArray();
        var agents = _agents.Where(x => candidates.Contains(x.Key.Name)).Select(x => x.Key).ToArray();
        msgs ??= _messages.ToArray();

        _logger.LogInformation("Getting next agent speaker");
        var nextSpeaker = agents.Length == 1 ? agents[0] : await GetNextAgentSpeaker(agents, msgs);
        if (nextSpeaker is null)
        {
            _logger.LogInformation("No next speaker found");
            return null;
        }

        _logger.LogInformation("Next Speaker: {NextSpeaker}", nextSpeaker.Name);
    
        var nextSpeakerObserver = _agents[nextSpeaker];

        var reply = await nextSpeakerObserver.GenerateReplyAsync(nextSpeaker, msgs, _channelInfo);

        if (reply is not null)
        {
            _logger.LogInformation("Generated reply: {Reply}", reply.GetContent());

            await this.SendMessage(reply);
        }
        return reply;
    }
}
