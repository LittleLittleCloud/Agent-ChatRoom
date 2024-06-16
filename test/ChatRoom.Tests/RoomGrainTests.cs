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

        var testChannel = new ChannelInfo(nameof(ItCreateChannelAsync));
        await roomGrain.CreateChannel(testChannel.Name);

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
        Mock.Get(observer).Setup(x => x.JoinRoom(It.IsAny<AgentInfo>(), It.IsAny<string>()))
            .Callback<AgentInfo, string>((agentInfo, _) =>
            {
                agents.Add(agentInfo);
                hasCompleted = true;
            });
        Mock.Get(observer).Setup(x => x.LeaveRoom(It.IsAny<AgentInfo>(), It.IsAny<string>()))
            .Callback<AgentInfo, string>((agentInfo, _) =>
            {
                agents.Remove(agentInfo);
                hasCompleted = true;
            });

        var observerRef = _cluster.Client.CreateObjectReference<IRoomObserver>(observer);
        var observerAgent = new AgentInfo("observe-agent", "observe-agent", false);
        await roomGrain.JoinRoom("observe-agent", "observe-agent", false, observerRef);
        await chatPlatformClient.RegisterAgentAsync(agent);

        await Utils.WaitUntilTrue(() => hasCompleted);
        var members = await roomGrain.GetMembers();
        members.Should().HaveCount(2);
        agents.Should().HaveCount(2);

        hasCompleted = false;
        await chatPlatformClient.UnregisterAgentAsync(agent);
        await Utils.WaitUntilTrue(() => hasCompleted);
        members = await roomGrain.GetMembers();
        members.Should().HaveCount(1);
    }

    [Fact]
    public async Task ItCreateChannelWithChatHistoryTestAsync()
    {
        var roomGrain = _cluster.GrainFactory.GetGrain<IRoomGrain>(nameof(ItCreateChannelWithChatHistoryTestAsync));
        var agent = new DummyAgent(new AgentInfo("test-agent", "test-agent", false));
        var chatHistory = new List<ChatMsg>
        {
            new ChatMsg("test-agent", "test message")
        };

        var observer = Mock.Of<IRoomObserver>();
        await roomGrain.JoinRoom(agent.Name, "test-agent", false, _cluster.Client.CreateObjectReference<IRoomObserver>(observer));
        await roomGrain.CreateChannel(nameof(ItCreateChannelWithChatHistoryTestAsync), [agent.Name], chatHistory.ToArray());
        var channelGrain = _cluster.GrainFactory.GetGrain<IChannelGrain>(nameof(ItCreateChannelWithChatHistoryTestAsync));

        var history = await channelGrain.ReadHistory(10);
        history.Should().HaveCount(1);
        var member = await channelGrain.GetMembers();
        member.Should().HaveCount(1);
    }

    [Fact]
    public async Task ItDeleteChatMessageFromChannelTestAsync()
    {
        var roomGrain = _cluster.GrainFactory.GetGrain<IRoomGrain>(nameof(ItCreateChannelWithChatHistoryTestAsync));
        var agent = new DummyAgent(new AgentInfo("test-agent", "test-agent", false));
        var message = new ChatMsg("test-agent", "test message");
        message.ID.Should().NotBe(0);
        var chatHistory = new List<ChatMsg>
        {
            message,
        };

        var observer = Mock.Of<IRoomObserver>();
        await roomGrain.JoinRoom(agent.Name, "test-agent", false, _cluster.Client.CreateObjectReference<IRoomObserver>(observer));
        await roomGrain.CreateChannel(nameof(ItDeleteChatMessageFromChannelTestAsync), [agent.Name], chatHistory.ToArray());
        var channelGrain = _cluster.GrainFactory.GetGrain<IChannelGrain>(nameof(ItDeleteChatMessageFromChannelTestAsync));

        var history = await channelGrain.ReadHistory(10);
        history.Should().HaveCount(1);
        var member = await channelGrain.GetMembers();
        member.Should().HaveCount(1);

        // edit message
        await channelGrain.EditTextMessage(message.ID, "new message");
        history = await channelGrain.ReadHistory(1);
        history.Should().HaveCount(1);
        history.First().GetContent().Should().Be("new message");
        // ID needs to be match
        history.First().ID.Should().Be(message.ID);

        // delete message
        await channelGrain.DeleteMessage(message.ID);
        history = await channelGrain.ReadHistory(1);
        history.Should().BeEmpty();
    }

    [Fact]
    public async Task AgentsJoinAndLeaveChannelTestAsync()
    {
        var roomGrain = _cluster.GrainFactory.GetGrain<IRoomGrain>(nameof(AgentsJoinAndLeaveChannelTestAsync));
        var chatPlatformClient = new ChatPlatformClient(_cluster.Client, nameof(AgentsJoinAndLeaveChannelTestAsync));
        var testChannelName = nameof(AgentsJoinAndLeaveChannelTestAsync);
        await roomGrain.CreateChannel(testChannelName);

        var agentInfo = new AgentInfo("test-agent", "test-agent", false);
        var agent = new DummyAgent(agentInfo);
        var agents = new List<AgentInfo>();

        var observeAgentInfo = new AgentInfo("observe-agent", "observe-agent", false);
        var roomObserver = Mock.Of<IRoomObserver>();
        var agentInfoList = new List<AgentInfo>();
        bool hasCompleted = false;
        bool hasJoinRoomCompleted = false;
        Mock.Get(roomObserver).Setup(x => x.JoinChannel(It.IsAny<AgentInfo>(), It.IsAny<ChannelInfo>()))
            .Callback<AgentInfo, ChannelInfo>((agentInfo, _) =>
            {
                agentInfoList.Add(agentInfo);
                hasCompleted = true;
            });

        Mock.Get(roomObserver).Setup(x => x.LeaveChannel(It.IsAny<AgentInfo>(), It.IsAny<ChannelInfo>()))
            .Callback<AgentInfo, ChannelInfo>((agentInfo, _) =>
            {
                agentInfoList.Remove(agentInfo);
                hasCompleted = true;
            });

        Mock.Get(roomObserver).Setup(x => x.JoinRoom(It.IsAny<AgentInfo>(), It.IsAny<string>()))
            .Callback<AgentInfo, string>((agentInfo, _) =>
            {
                agents.Add(agentInfo);
                hasJoinRoomCompleted = true;
            });
        Mock.Get(roomObserver).Setup(x => x.LeaveRoom(It.IsAny<AgentInfo>(), It.IsAny<string>()))
            .Callback<AgentInfo, string>((agentInfo, _) =>
            {
                agents.Remove(agentInfo);
                hasJoinRoomCompleted = true;
            });

        var observerRef = _cluster.Client.CreateObjectReference<IRoomObserver>(roomObserver);
        await roomGrain.JoinRoom("observe-agent", "observe-agent", false, observerRef);
        await Utils.WaitUntilTrue(() => hasJoinRoomCompleted);
        hasJoinRoomCompleted = false;
        await chatPlatformClient.RegisterAgentAsync(agent, agentInfo.SelfDescription);
        await Utils.WaitUntilTrue(() => hasJoinRoomCompleted);
        await roomGrain.AddAgentToChannel(testChannelName, "observe-agent");
        await Utils.WaitUntilTrue(() => hasCompleted);
        hasCompleted = false;
        //agentInfoList.Clear();

        // join the room
        // join the channel
        await roomGrain.AddAgentToChannel(testChannelName, agentInfo.Name);

        await Utils.WaitUntilTrue(() => hasCompleted);
        agentInfoList.Should().HaveCount(2);

        // leave the channel
        hasCompleted = false;

        await roomGrain.RemoveAgentFromChannel(testChannelName, agentInfo.Name);

        await Utils.WaitUntilTrue(() => hasCompleted);
        agentInfoList.Should().HaveCount(1);
    }

    [Fact]
    public async Task AgentJoinAndLeaveRoomTestAsync()
    {
        var roomGrain = _cluster.GrainFactory.GetGrain<IRoomGrain>(nameof(AgentJoinAndLeaveRoomTestAsync));
        var chatPlatformClient = new ChatPlatformClient(_cluster.Client, nameof(AgentJoinAndLeaveRoomTestAsync));
        var testChannel = nameof(AgentJoinAndLeaveRoomTestAsync);
        await roomGrain.CreateChannel(testChannel);

        var agentInfo = new AgentInfo("test-agent", "test-agent", false);
        var agent = new DummyAgent(agentInfo);

        var observeAgentInfo = new AgentInfo("observe-agent", "observe-agent", false);
        var agentObserver = Mock.Of<IRoomObserver>();
        var agentInfoList = new List<AgentInfo>();
        var hasCompleted = false;
        Mock.Get(agentObserver).Setup(x => x.JoinChannel(It.IsAny<AgentInfo>(), It.IsAny<ChannelInfo>()))
            .Callback<AgentInfo, ChannelInfo>((agentInfo, _) =>
            {
                agentInfoList.Add(agentInfo);
                hasCompleted = true;
            });

        Mock.Get(agentObserver).Setup(x => x.LeaveChannel(It.IsAny<AgentInfo>(), It.IsAny<ChannelInfo>()))
            .Callback<AgentInfo, ChannelInfo>((agentInfo, _) =>
            {
                agentInfoList.Remove(agentInfo);
                hasCompleted = true;
            });
        var agentObserverRef = _cluster.Client.CreateObjectReference<IRoomObserver>(agentObserver);
        await roomGrain.JoinRoom("observe-agent", "observe-agent", false, agentObserverRef);
        await roomGrain.AddAgentToChannel(testChannel, observeAgentInfo.Name);
        await Utils.WaitUntilTrue(() => hasCompleted);

        hasCompleted = false;
        agentInfoList.Clear();
        // join the room
        await chatPlatformClient.RegisterAgentAsync(agent, agentInfo.SelfDescription);
        // join the channel
        await roomGrain.AddAgentToChannel(testChannel, agentInfo.Name);

        await Utils.WaitUntilTrue(() => hasCompleted);

        agentInfoList.Should().HaveCount(1);
        agentInfoList.First().Should().BeEquivalentTo(agentInfo);

        hasCompleted = false;
        // leave the room, in which case the agent should leave the channel as well
        await chatPlatformClient.UnregisterAgentAsync(agent);

        await Utils.WaitUntilTrue(() => hasCompleted);
        agentInfoList.Should().BeEmpty();
        var members = await roomGrain.GetMembers();
        members.Should().HaveCount(1);
    }

    [Fact]
    public async Task AgentIsAliveTestAsync()
    {
        var roomGrain = _cluster.GrainFactory.GetGrain<IRoomGrain>(nameof(AgentIsAliveTestAsync));
        var chatPlatformClient = new ChatPlatformClient(_cluster.Client, nameof(AgentIsAliveTestAsync));

        var testAgent = new AgentInfo("test-agent", "test-agent", false);
        var agent = new DummyAgent(testAgent);

        var testChannel = nameof(AgentIsAliveTestAsync);

        await chatPlatformClient.RegisterAgentAsync(agent, testAgent.SelfDescription);
        await roomGrain.CreateChannel(testChannel);
        await roomGrain.AddAgentToChannel(testChannel, testAgent.Name);

        var channelGrain = _cluster.GrainFactory.GetGrain<IChannelGrain>(testChannel);
        //var alivedMembers = await channelGrain.GetAli();
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
        hostBuilder
            .AddMemoryGrainStorage("PubSubStore")
            .Services.AddSingleton<ChannelConfiguration>();
    }
}
