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

namespace ChatRoom.Tests;

[Collection(ClusterFixtureCollection.Name)]
public class RoomGrainTests(ClusterFixture fixture)
{
    private readonly TestCluster _cluster = fixture.Cluster;

    [Fact]
    public async Task ItCreateChannelAsync()
    {
        var roomGrain = _cluster.GrainFactory.GetGrain<IRoomGrain>("room");

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
        var notificationMsg = new List<ChatMsg>();
        // add mock implementation for Notification which adds the message to the list
        Mock.Get(observer).Setup(x => x.Notification(It.IsAny<ChatMsg>()))
            .Callback<ChatMsg>(notificationMsg.Add);
        var observerRef = _cluster.Client.CreateObjectReference<IRoomObserver>(observer);
        await roomGrain.Subscribe(observerRef);
        await chatPlatformClient.RegisterAgentAsync(agent);

        var members = roomGrain.GetMembers().Result;
        members.Should().HaveCount(1);
        notificationMsg.Should().HaveCount(1);
        var notification = notificationMsg.First();
        notification.From.Should().Be("System");
        notification.Text.Should().Be($"{agentInfo.Name} joins the chat room.");

        notificationMsg.Clear();

        await chatPlatformClient.UnregisterAgentAsync(agent);
        members = roomGrain.GetMembers().Result;
        members.Should().BeEmpty();
        notificationMsg.Should().HaveCount(1);
        notification = notificationMsg.First();
        notification.From.Should().Be("System");
        notification.Text.Should().Be($"{agentInfo.Name} leaves the chat room.");
    }

    [Fact]
    public async Task AgentsJoinAndLeaveChannelTestAsync()
    {
        var roomGrain = _cluster.GrainFactory.GetGrain<IRoomGrain>(nameof(AgentsJoinAndLeaveChannelTestAsync));
        var chatPlatformClient = new ChatPlatformClient(_cluster.Client, nameof(AgentsJoinAndLeaveChannelTestAsync));
        var agentInfo = new AgentInfo("test-agent", "test-agent", false);
        var agent = new DummyAgent(agentInfo);
        var roomObserver = Mock.Of<IRoomObserver>();
        var roomObserverMessages = new List<ChatMsg>();
        // add mock implementation for Notification which adds the message to the list
        Mock.Get(roomObserver).Setup(x => x.Notification(It.IsAny<ChatMsg>()))
            .Callback<ChatMsg>(roomObserverMessages.Add);
        var observerRef = _cluster.Client.CreateObjectReference<IRoomObserver>(roomObserver);
        await roomGrain.Subscribe(observerRef);

        var testChannel = new ChannelInfo("test-channel");
        await roomGrain.CreateChannel(testChannel);
        var channelObserver = Mock.Of<IChannelObserver>();
        var agentInfoList = new List<AgentInfo>();
        Mock.Get(channelObserver).Setup(x => x.Join(It.IsAny<AgentInfo>()))
            .Callback<AgentInfo>(agentInfoList.Add);

        Mock.Get(channelObserver).Setup(x => x.Leave(It.IsAny<AgentInfo>()))
            .Callback<AgentInfo>((agentInfo) => agentInfoList.Remove(agentInfo));
        var channelObserverRef = _cluster.Client.CreateObjectReference<IChannelObserver>(channelObserver);
        var channelGrain = _cluster.GrainFactory.GetGrain<IChannelGrain>(testChannel.Name);
        await channelGrain.Subscribe(channelObserverRef);

        // join the room
        await chatPlatformClient.RegisterAgentAsync(agent, agentInfo.SelfDescription);

        // join the channel
        await roomGrain.AddAgentToChannel(testChannel, agentInfo);

        agentInfoList.Should().HaveCount(1);
        agentInfoList.First().Should().BeEquivalentTo(agentInfo);

        // leave the channel
        await roomGrain.RemoveAgentFromChannel(testChannel, agentInfo);
        agentInfoList.Should().BeEmpty();
    }

    [Fact]
    public async Task AgentJoinAndLeaveRoomTestAsync()
    {
        var roomGrain = _cluster.GrainFactory.GetGrain<IRoomGrain>(nameof(AgentJoinAndLeaveRoomTestAsync));
        var chatPlatformClient = new ChatPlatformClient(_cluster.Client, nameof(AgentJoinAndLeaveRoomTestAsync));
        var agentInfo = new AgentInfo("test-agent", "test-agent", false);
        var agent = new DummyAgent(agentInfo);
        var roomObserver = Mock.Of<IRoomObserver>();
        var roomObserverMessages = new List<ChatMsg>();
        // add mock implementation for Notification which adds the message to the list
        Mock.Get(roomObserver).Setup(x => x.Notification(It.IsAny<ChatMsg>()))
            .Callback<ChatMsg>(roomObserverMessages.Add);
        var observerRef = _cluster.Client.CreateObjectReference<IRoomObserver>(roomObserver);
        await roomGrain.Subscribe(observerRef);

        var testChannel = new ChannelInfo("test-channel");
        await roomGrain.CreateChannel(testChannel);
        var channelObserver = Mock.Of<IChannelObserver>();
        var agentInfoList = new List<AgentInfo>();
        Mock.Get(channelObserver).Setup(x => x.Join(It.IsAny<AgentInfo>()))
            .Callback<AgentInfo>(agentInfoList.Add);

        Mock.Get(channelObserver).Setup(x => x.Leave(It.IsAny<AgentInfo>()))
            .Callback<AgentInfo>((agentInfo) => agentInfoList.Remove(agentInfo));
        var channelObserverRef = _cluster.Client.CreateObjectReference<IChannelObserver>(channelObserver);
        var channelGrain = _cluster.GrainFactory.GetGrain<IChannelGrain>(testChannel.Name);
        await channelGrain.Subscribe(channelObserverRef);

        // join the room
        await chatPlatformClient.RegisterAgentAsync(agent, agentInfo.SelfDescription);

        // join the channel
        await roomGrain.AddAgentToChannel(testChannel, agentInfo);

        agentInfoList.Should().HaveCount(1);
        agentInfoList.First().Should().BeEquivalentTo(agentInfo);

        // leave the room, in which case the agent should leave the channel as well
        await chatPlatformClient.UnregisterAgentAsync(agent);

        agentInfoList.Should().BeEmpty();
        var members = await roomGrain.GetMembers();
        members.Should().BeEmpty();
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
            .AddMemoryGrainStorage("PubSubStore");
    }
}
