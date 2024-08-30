using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoGen.Core;
using ChatRoom.SDK;
using FluentAssertions;
using Moq;
using Orleans.TestingHost;
using Xunit;

namespace ChatRoom.Tests;

[Collection(ClusterFixtureCollection.Name)]
public class ChannelGrainTests(ClusterFixture fixture)
{
    private readonly TestCluster _cluster = fixture.Cluster;

    [Fact]
    public async Task ItAddAndRemoveOrchestratorTest()
    {
        var channel = _cluster.GrainFactory.GetGrain<IChannelGrain>(nameof(ItAddAndRemoveOrchestratorTest));
        var orchestrator = Mock.Of<IOrchestratorObserver>();
        var orchestratorRef = _cluster.GrainFactory.CreateObjectReference<IOrchestratorObserver>(orchestrator);

        await channel.AddOrchestratorToChannel("orchestrator", orchestratorRef);

        var channelInfo = await channel.GetChannelInfo();
        channelInfo.Orchestrators.Should().ContainSingle(x => x == "orchestrator");

        await channel.RemoveOrchestratorFromChannel("orchestrator");
        channelInfo = await channel.GetChannelInfo();

        channelInfo.Orchestrators.Should().NotContain("orchestrator");
    }


    [Fact]
    public async Task ItUseOrchstratorTest()
    {
        var channel = _cluster.GrainFactory.GetGrain<IChannelGrain>(nameof(ItUseOrchstratorTest));
        var orchestrator = Mock.Of<IOrchestratorObserver>();
        Mock.Get(orchestrator).Setup(x => x.GetNextSpeaker(It.IsAny<AgentInfo[]>(), It.IsAny<ChatMsg[]>()))
            .ReturnsAsync("human");
        var orchestratorRef = _cluster.GrainFactory.CreateObjectReference<IOrchestratorObserver>(orchestrator);

        var human = Mock.Of<IChatRoomAgentObserver>();
        Mock.Get(human).Setup(x => x.GenerateReplyAsync(It.IsAny<AgentInfo>(), It.IsAny<ChatMsg[]>(), It.IsAny<ChannelInfo>()))
            .ReturnsAsync(new ChatMsg("human", "I am human"));
        var humanRef = _cluster.GrainFactory.CreateObjectReference<IChatRoomAgentObserver>(human);
        
        var bot = Mock.Of<IChatRoomAgentObserver>();
        Mock.Get(bot).Setup(x => x.GenerateReplyAsync(It.IsAny<AgentInfo>(), It.IsAny<ChatMsg[]>(), It.IsAny<ChannelInfo>()))
            .ReturnsAsync(new ChatMsg("bot", "I am bot"));
        var botRef = _cluster.GrainFactory.CreateObjectReference<IChatRoomAgentObserver>(bot);

        await channel.AddAgentToChannel("human", "dummy", true, humanRef);
        await channel.AddAgentToChannel("bot", "bot", false, botRef);
        await channel.AddOrchestratorToChannel("orchestrator", orchestratorRef);

        var channelInfo = await channel.GetChannelInfo();
        channelInfo.Orchestrators.Should().ContainSingle(x => x == "orchestrator");

        var reply = await channel.GenerateNextReply(orchestrator: "orchestrator");
        reply!.From.Should().Be("human");

        reply = await channel.GenerateNextReply(orchestrator: "orchestrator");
        reply!.From.Should().Be("human");

        var chatHistory = await channel.ReadHistory(1000);
        chatHistory.Count().Should().Be(2);
    }

}
