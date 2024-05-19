using AutoGen.Core;
using Azure.AI.OpenAI;
using ChatRoom.Common;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Orleans.Runtime;
using Orleans.Streams;
using Orleans.Utilities;

namespace ChatRoom.Room;

internal class ChannelGrain : Grain, IChannelGrain
{
    private readonly List<ChatMsg> _messages = new(100);
    private readonly List<AgentInfo> _onlineMembers = new(10);
    private ChannelInfo _channelInfo = null!;
    private readonly ObserverManager<IChannelObserver> _channelObserver;

    public ChannelGrain(ILogger<RoomGrain> logger)
    {
        _channelObserver = new ObserverManager<IChannelObserver>(TimeSpan.FromMinutes(1), logger);
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _channelInfo = new ChannelInfo(this.GetPrimaryKeyString());
        await base.OnActivateAsync(cancellationToken);
        var roomGrain = this.GrainFactory.GetGrain<IRoomGrain>("room");
        await roomGrain.CreateChannel(_channelInfo);
    }

    public async Task Join(AgentInfo agentInfo)
    {
        _onlineMembers.Add(agentInfo);
        await _channelObserver.Notify(x => x.Join(agentInfo));
    }

    public async Task Leave(AgentInfo agentInfo)
    {
        _onlineMembers.Remove(agentInfo);
        await _channelObserver.Notify(x => x.Leave(agentInfo));
    }

    public async Task<bool> Message(ChatMsg msg)
    {
        await _channelObserver.Notify(x => x.NewMessage(msg));
        if (msg.From != "System")
        {
            _messages.Add(msg);
            await GetNextAgentSpeaker();
        }

        return true;
    }

    public Task<AgentInfo[]> GetMembers() => Task.FromResult(_onlineMembers.ToArray());

    public Task<ChatMsg[]> ReadHistory(int numberOfMessages)
    {
        var response = _messages
            .OrderByDescending(x => x.Created)
            .Take(numberOfMessages)
            .OrderBy(x => x.Created)
            .ToArray();

        return Task.FromResult(response);
    }

    public async Task<AgentInfo?> GetNextAgentSpeaker()
    {
        var humanMembers = _onlineMembers.Where(x => x.IsHuman).ToArray();
        var notHumanMembers = _onlineMembers.Where(x => !x.IsHuman).ToArray();
        var humanAgents = humanMembers.Select(x => new DummyAgent(x)).ToArray();
        var notHumanAgents = notHumanMembers.Select(x => new DummyAgent(x)).ToArray();
        var agents = humanAgents.Concat(notHumanAgents).ToArray();
        // create agents
        var AZURE_OPENAI_ENDPOINT = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
        var AZURE_OPENAI_KEY = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
        var AZURE_DEPLOYMENT_NAME = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOY_NAME");
        var OPENAI_API_KEY = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        var OPENAI_MODEL_ID = Environment.GetEnvironmentVariable("OPENAI_MODEL_ID") ?? "gpt-3.5-turbo-0125";

        OpenAIClient openaiClient;
        bool useAzure = false;
        if (AZURE_OPENAI_ENDPOINT is string && AZURE_OPENAI_KEY is string && AZURE_DEPLOYMENT_NAME is string)
        {
            openaiClient = new OpenAIClient(new Uri(AZURE_OPENAI_ENDPOINT), new Azure.AzureKeyCredential(AZURE_OPENAI_KEY));
            useAzure = true;
        }
        else if (OPENAI_API_KEY is string)
        {
            openaiClient = new OpenAIClient(OPENAI_API_KEY);
        }
        else
        {
            throw new ArgumentException("Please provide either (AZURE_OPENAI_ENDPOINT, AZURE_OPENAI_KEY, AZURE_DEPLOYMENT_NAME) or OPENAI_API_KEY");
        }

        var deployModelName = useAzure ? AZURE_DEPLOYMENT_NAME! : OPENAI_MODEL_ID;

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
        var chatHistory = _messages.Select(x => new TextMessage(Role.Assistant, x.Text, x.From)).ToArray();
        var lastSpeaker = agents.First(x => x.Name == _messages.Last().From);
        var nextAvailableAgents = await graph.TransitToNextAvailableAgentsAsync(lastSpeaker, _messages);
        if (nextAvailableAgents.Count() == 1 && _onlineMembers.Any(x => x.Name == nextAvailableAgents.First().Name))
        {
            var nextSpeaker = _onlineMembers.First(x => x.Name == nextAvailableAgents.First().Name);
            await _channelObserver.Notify(x => GenerateNextReply(x, nextSpeaker, _messages.ToArray()));
            return nextSpeaker;
        }
        else if (nextAvailableAgents.Count() > 1)
        {
            IAgent admin = AgentFactory.CreateGroupChatAdmin(openaiClient, modelName: "gpt-4");
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
            var nextSpeaker = _onlineMembers.First(x => x.Name == lastMessage.From);
            if (nextAvailableAgents.Any(x => x.Name == nextSpeaker.Name))
            {
                await _channelObserver.Notify(x => GenerateNextReply(x, nextSpeaker, _messages.ToArray()));
                return nextSpeaker;
            }

            return null;
        }
        else
        {
            return null;
        }
    }

    public async Task Subscribe(IChannelObserver observer)
    {
        this._channelObserver.Subscribe(observer, observer);
    }

    public async Task Unsubscribe(IChannelObserver observer)
    {
        this._channelObserver.Unsubscribe(observer);
    }

    [OneWay]
    private async Task GenerateNextReply(IChannelObserver observer, AgentInfo agent, ChatMsg[] msg)
    {
        var reply = await observer.GenerateReplyAsync(agent, msg);

        if (reply is not null)
        {
            await this.Message(reply);
        }
    }
}
