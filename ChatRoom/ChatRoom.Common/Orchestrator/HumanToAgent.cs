using AutoGen.Core;

namespace ChatRoom.SDK;

internal class HumanToAgent : IOrchestrator
{
    private readonly OpenAIClientConfiguration _config;
    public HumanToAgent(OpenAIClientConfiguration llmConfig)
    {
        _config = llmConfig;
    }

    public async Task<string?> GetNextSpeaker(AgentInfo[] members, ChatMsg[] messages)
    {
        if (messages is { Length: 0 })
        {
            return null;
        }

        // dumb implementation by using AutoGen GroupChat
        var humanMembers = members.Where(x => x.IsHuman).ToArray();
        var notHumanMembers = members.Where(x => !x.IsHuman).ToArray();
        var humanAgents = humanMembers.Select(x => new DummyAgent(x)).ToArray();
        var notHumanAgents = notHumanMembers.Select(x => new DummyAgent(x)).ToArray();
        var agents = humanAgents.Concat(notHumanAgents).ToArray();
        // create agents
        var openaiClient = _config.ToOpenAIClient();
        var deployModelName = _config.ModelId;

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
        var nextAvailableAgents = await graph.TransitToNextAvailableAgentsAsync(lastSpeaker, messages);
        if (nextAvailableAgents.Count() == 1 && members.Any(x => x.Name == nextAvailableAgents.First().Name))
        {
            var nextSpeaker = members.First(x => x.Name == nextAvailableAgents.First().Name);
            return nextSpeaker.Name;
        }
        else if (nextAvailableAgents.Count() > 1)
        {
            IAgent admin = OpenAIAgentFactory.CreateGroupChatAdmin(openaiClient, modelName: deployModelName);
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
                return nextSpeaker.Name;
            }

            return null;
        }
        else
        {
            return null;
        }
    }
}
