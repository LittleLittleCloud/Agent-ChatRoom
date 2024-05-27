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
        var testChannel = new ChannelInfo(nameof(AgentsJoinAndLeaveChannelTestAsync));
        await roomGrain.CreateChannel(testChannel);

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
        await WaitUntilTrue(() => hasJoinRoomCompleted);
        hasJoinRoomCompleted = false;
        await chatPlatformClient.RegisterAgentAsync(agent, agentInfo.SelfDescription);
        await WaitUntilTrue(() => hasJoinRoomCompleted);
        await roomGrain.AddAgentToChannel(testChannel, "observe-agent");
        await WaitUntilTrue(() => hasCompleted);
        hasCompleted = false;
        //agentInfoList.Clear();

        // join the room
        // join the channel
        await roomGrain.AddAgentToChannel(testChannel, agentInfo.Name);

        await WaitUntilTrue(() => hasCompleted);
        agentInfoList.Should().HaveCount(2);

        // leave the channel
        hasCompleted = false;

        await roomGrain.RemoveAgentFromChannel(testChannel, agentInfo.Name);

        await WaitUntilTrue(() => hasCompleted);
        agentInfoList.Should().HaveCount(1);
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
        await WaitUntilTrue(() => hasCompleted);

        hasCompleted = false;
        agentInfoList.Clear();
        // join the room
        await chatPlatformClient.RegisterAgentAsync(agent, agentInfo.SelfDescription);
        // join the channel
        await roomGrain.AddAgentToChannel(testChannel, agentInfo.Name);

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

    [Fact]
    public async Task AgentIsAliveTestAsync()
    {
        var roomGrain = _cluster.GrainFactory.GetGrain<IRoomGrain>(nameof(AgentIsAliveTestAsync));
        var chatPlatformClient = new ChatPlatformClient(_cluster.Client, nameof(AgentIsAliveTestAsync));

        var testAgent = new AgentInfo("test-agent", "test-agent", false);
        var agent = new DummyAgent(testAgent);

        var testChannel = new ChannelInfo("test-channel");

        await chatPlatformClient.RegisterAgentAsync(agent, testAgent.SelfDescription);
        await roomGrain.CreateChannel(testChannel);
        await roomGrain.AddAgentToChannel(testChannel, testAgent.Name);

        var channelGrain = _cluster.GrainFactory.GetGrain<IChannelGrain>(testChannel.Name);
        //var alivedMembers = await channelGrain.GetAli();
    }

    private Task WaitUntilTrue(Func<bool> condition, int maxSeconds = 10)
    {
        var timeout = TimeSpan.FromSeconds(maxSeconds);
        return Task.Run(() =>
        {
            while (!condition())
            {
                Task.Delay(100).Wait();
                timeout -= TimeSpan.FromMilliseconds(100);

                if (timeout <= TimeSpan.Zero)
                {
                    throw new TimeoutException("Condition was not met within the timeout");
                }
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
