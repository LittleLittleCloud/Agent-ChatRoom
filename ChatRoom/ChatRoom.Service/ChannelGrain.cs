using AutoGen.Core;
using Azure.AI.OpenAI;
using ChatRoom.Common;
using Orleans.Runtime;
using Orleans.Streams;

namespace ChatRoom.Room;

public class ChannelGrain : Grain, IChannelGrain
{
    private readonly List<ChatMsg> _messages = new(100);
    private readonly List<AgentInfo> _onlineMembers = new(10);
    private IAsyncStream<ChatMsg> _chatMsgStream = null!;
    private IAsyncStream<AgentInfo> _agentInfoStream = null!;
    private ChannelInfo _channelInfo = null!;
    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _channelInfo = new ChannelInfo(this.GetPrimaryKeyString());
        var streamProvider = this.GetStreamProvider("chat");

        var chatStreamId = StreamId.Create(
            "ChatRoom", this.GetPrimaryKeyString());

        var agentInfoStreamId = StreamId.Create(
            "AgentInfo", this.GetPrimaryKeyString());

        _chatMsgStream = streamProvider.GetStream<ChatMsg>(
            chatStreamId);
        _agentInfoStream = streamProvider.GetStream<AgentInfo>(agentInfoStreamId);

        await base.OnActivateAsync(cancellationToken);
        var roomGrain = this.GrainFactory.GetGrain<IRoomGrain>("room");
        await roomGrain.CreateChannel(_channelInfo);
    }

    public async Task<StreamId> Join(AgentInfo agentInfo)
    {
        if (_onlineMembers.Contains(agentInfo))
        {
            await _chatMsgStream.OnNextAsync(
                new ChatMsg(
                    "System",
                    $"{agentInfo} is already in the chat '{this.GetPrimaryKeyString()}' ..."));
            return _chatMsgStream.StreamId;
        }

        _onlineMembers.Add(agentInfo);

        await _chatMsgStream.OnNextAsync(
            new ChatMsg(
                "System",
                $"{agentInfo} joins the chat '{this.GetPrimaryKeyString()}' ..."));

        return _chatMsgStream.StreamId;
    }

    public async Task<StreamId> Leave(AgentInfo agentInfo)
    {
        _onlineMembers.Remove(agentInfo);

        await _chatMsgStream.OnNextAsync(
            new ChatMsg(
                "System",
                $"{agentInfo} leaves the chat..."));

        return _chatMsgStream.StreamId;
    }

    public async Task<bool> Message(ChatMsg msg)
    {
        await _chatMsgStream.OnNextAsync(msg);
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

            await _agentInfoStream.OnNextAsync(nextSpeaker);

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
                await _agentInfoStream.OnNextAsync(nextSpeaker);

                return nextSpeaker;
            }

            return null;

        }
        else
        {
            return null;
        }
    }
}
