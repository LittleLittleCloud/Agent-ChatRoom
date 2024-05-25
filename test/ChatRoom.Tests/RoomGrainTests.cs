using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApprovalTests.Utilities;
using Xunit;
using Orleans.TestingHost;
using ChatRoom.Common;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using ChatRoom.Room;

namespace ChatRoom.Tests;

[Collection(ClusterFixtureCollection.Name)]
public class RoomGrainTests(ClusterFixture fixture)
{
    private readonly TestCluster _cluster = fixture.Cluster;

    [Fact]
    public async Task ItCreateChannelAsync()
    {
        var roomGrain = _cluster.GrainFactory.GetGrain<IRoomGrain>(nameof(ItCreateChannelAsync));

        roomGrain.GetChannels().Result.Should().BeEmpty();

        var testChannel = new ChannelInfo("test-channel");
        await roomGrain.CreateChannel(testChannel);

        var channels = roomGrain.GetChannels().Result;
        channels.Should().HaveCount(1);
    }

    [Fact]
    public async Task ItRegisterAndUnregisterAgentToRoomAsync()
    {
        var roomGrain = _cluster.GrainFactory.GetGrain<IRoomGrain>(nameof(ItRegisterAndUnregisterAgentToRoomAsync));
        var chatPlatformClient = new ChatPlatformClient(_cluster.Client, nameof(ItRegisterAndUnregisterAgentToRoomAsync));
        var agentInfo = new AgentInfo("test-agent", "test-agent", false);
        var agent = new DummyAgent(agentInfo);
        var observer = Mock.Of<IRoomObserver>();
        var agents = new List<AgentInfo>();
        // add mock implementation for Notification which adds the message to the list
        bool hasCompleted = false;
        Mock.Get(observer).Setup(x => x.Join(It.IsAny<AgentInfo>(), It.IsAny<string>()))
            .Callback<AgentInfo, string>((agentInfo, _) =>
            {
                agents.Add(agentInfo);
                hasCompleted = true;
            });
        Mock.Get(observer).Setup(x => x.Leave(It.IsAny<AgentInfo>(), It.IsAny<string>()))
            .Callback<AgentInfo, string>((agentInfo, _) =>
            {
                agents.Remove(agentInfo);
                hasCompleted = true;
            });

        var observerRef = _cluster.Client.CreateObjectReference<IRoomObserver>(observer);
        var observerAgent = new AgentInfo("observe-agent", "observe-agent", false);
        await roomGrain.Join(observerAgent, observerRef);
        await chatPlatformClient.RegisterAgentAsync(agent);

        await WaitUntilTrue(() => hasCompleted);
        var members = await roomGrain.GetMembers();
        members.Should().HaveCount(2);
        agents.Should().HaveCount(2);

        hasCompleted = false;
        await chatPlatformClient.UnregisterAgentAsync(agent);
        await WaitUntilTrue(() => hasCompleted);
        members = await roomGrain.GetMembers();
        members.Should().HaveCount(1);
    }

    [Fact]
    public async Task AgentsJoinAndLeaveChannelTestAsync()
    {
        var roomGrain = _cluster.GrainFactory.GetGrain<IRoomGrain>(nameof(AgentsJoinAndLeaveChannelTestAsync));
        var chatPlatformClient = new ChatPlatformClient(_cluster.Client, nameof(AgentsJoinAndLeaveChannelTestAsync));
        var testChannel = new ChannelInfo("test-channel");
        await roomGrain.CreateChannel(testChannel);

        var agentInfo = new AgentInfo("test-agent", "test-agent", false);
        var agent = new DummyAgent(agentInfo);

        var observeAgentInfo = new AgentInfo("observe-agent", "observe-agent", false);
        var roomObserver = Mock.Of<IRoomObserver>();
        var roomObserverMessages = new List<ChatMsg>();
        // add mock implementation for Notification which adds the message to the list
        Mock.Get(roomObserver).Setup(x => x.Notification(It.IsAny<ChatMsg>()))
            .Callback<ChatMsg>(roomObserverMessages.Add);

        var agentInfoList = new List<AgentInfo>();
        bool hasCompleted = false;
        Mock.Get(roomObserver).Setup(x => x.Join(It.IsAny<AgentInfo>(), It.IsAny<ChannelInfo>()))
            .Callback<AgentInfo, ChannelInfo>((agentInfo, _) =>
            {
                agentInfoList.Add(agentInfo);
                hasCompleted = true;
            });

        Mock.Get(roomObserver).Setup(x => x.Leave(It.IsAny<AgentInfo>(), It.IsAny<ChannelInfo>()))
            .Callback<AgentInfo, ChannelInfo>((agentInfo, _) =>
            {
                agentInfoList.Remove(agentInfo);
                hasCompleted = true;
            });
        
        var observerRef = _cluster.Client.CreateObjectReference<IRoomObserver>(roomObserver);
        await roomGrain.Join(observeAgentInfo, observerRef);
        await roomGrain.AddAgentToChannel(testChannel, observeAgentInfo);
        hasCompleted = false;
        await WaitUntilTrue(() => hasCompleted);
        agentInfoList.Clear();
        // join the room
        await chatPlatformClient.RegisterAgentAsync(agent, agentInfo.SelfDescription);

        // join the channel
        await roomGrain.AddAgentToChannel(testChannel, agentInfo);

        await WaitUntilTrue(() => hasCompleted);
        agentInfoList.Should().HaveCount(1);
        agentInfoList.First().Should().BeEquivalentTo(agentInfo);

        // leave the channel
        hasCompleted = false;

        await roomGrain.RemoveAgentFromChannel(testChannel, agentInfo);

        await WaitUntilTrue(() => hasCompleted);
        agentInfoList.Should().BeEmpty();
    }

    [Fact]
    public async Task AgentJoinAndLeaveRoomTestAsync()
    {
        var roomGrain = _cluster.GrainFactory.GetGrain<IRoomGrain>(nameof(AgentJoinAndLeaveRoomTestAsync));
        var chatPlatformClient = new ChatPlatformClient(_cluster.Client, nameof(AgentJoinAndLeaveRoomTestAsync));
        var testChannel = new ChannelInfo("test-channel");
        await roomGrain.CreateChannel(testChannel);

        var agentInfo = new AgentInfo("test-agent", "test-agent", false);
        var agent = new DummyAgent(agentInfo);

        var observeAgentInfo = new AgentInfo("observe-agent", "observe-agent", false);
        var agentObserver = Mock.Of<IRoomObserver>();
        var agentInfoList = new List<AgentInfo>();
        var hasCompleted = false;
        Mock.Get(agentObserver).Setup(x => x.Join(It.IsAny<AgentInfo>(), It.IsAny<ChannelInfo>()))
            .Callback<AgentInfo, ChannelInfo>((agentInfo, _) =>
            {
                agentInfoList.Add(agentInfo);
                hasCompleted = true;
            });

        Mock.Get(agentObserver).Setup(x => x.Leave(It.IsAny<AgentInfo>(), It.IsAny<ChannelInfo>()))
            .Callback<AgentInfo, ChannelInfo>((agentInfo, _) =>
            {
                agentInfoList.Remove(agentInfo);
                hasCompleted = true;
            });
        var agentObserverRef = _cluster.Client.CreateObjectReference<IRoomObserver>(agentObserver);
        await roomGrain.Join(observeAgentInfo, agentObserverRef);
        await roomGrain.AddAgentToChannel(testChannel, observeAgentInfo);
        await WaitUntilTrue(() => hasCompleted);

        hasCompleted = false;
        agentInfoList.Clear();
        // join the room
        await chatPlatformClient.RegisterAgentAsync(agent, agentInfo.SelfDescription);
        // join the channel
        await roomGrain.AddAgentToChannel(testChannel, agentInfo);

        await WaitUntilTrue(() => hasCompleted);

        agentInfoList.Should().HaveCount(1);
        agentInfoList.First().Should().BeEquivalentTo(agentInfo);

        hasCompleted = false;
        // leave the room, in which case the agent should leave the channel as well
        await chatPlatformClient.UnregisterAgentAsync(agent);

        await WaitUntilTrue(() => hasCompleted);
        agentInfoList.Should().BeEmpty();
        var members = await roomGrain.GetMembers();
        members.Should().HaveCount(1);
    }

    private Task WaitUntilTrue(Func<bool> condition)
    {
        return Task.Run(() =>
        {
            while (!condition())
            {
                Task.Delay(100).Wait();
            }
        });
    }
}


public sealed class ClusterFixture : IDisposable
{
    public TestCluster Cluster { get; } = new TestClusterBuilder()
        .AddSiloBuilderConfigurator<TestSiloConfigurator>()
        .Build();

    public ClusterFixture() => Cluster.Deploy();

    void IDisposable.Dispose() => Cluster.StopAllSilos();
}

[CollectionDefinition(Name)]
public sealed class ClusterFixtureCollection : ICollectionFixture<ClusterFixture>
{
    public const string Name = nameof(ClusterFixtureCollection);
}

public sealed class TestSiloConfigurator : ISiloConfigurator
{
    public void Configure(ISiloBuilder hostBuilder)
    {
        hostBuilder.UseLocalhostClustering()
            .AddMemoryGrainStorage("PubSubStore")
            .Services.AddSingleton(new ChannelConfiguration()
            {
            });
    }
}
