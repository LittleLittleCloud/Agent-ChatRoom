using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using ChatRoom.Client.DTO;
using ChatRoom.SDK;
using ChatRoom.Room;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Newtonsoft.Json.Linq;
using Orleans.TestingHost;
using Xunit;
using static ChatRoom.Client.Tests.ChatRoomClientControllerTests;

namespace ChatRoom.Client.Tests;

[Collection(ClusterFixtureCollection.Name)]
public class ChatRoomClientControllerTests(ClusterFixture fixture)
{
    private readonly TestCluster _cluster = fixture.Cluster;
    private readonly ClientContext _clientContext = new ClientContext()
    {
        CurrentChannel = "test",
        CurrentRoom = nameof(ChatRoomClientControllerTests),
    };

    [Fact]
    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovalTests")]
    public async Task TestSwagger()
    {
        var webHostBuilder = new WebHostBuilder()
                         .UseEnvironment(Environments.Development) // You can set the environment you want (development, staging, production)
                         .UseStartup<Startup>(); // Startup class of your web app project

        using (var server = new TestServer(webHostBuilder))
        {
            using (var client = server.CreateClient())
            {
                string result = await client.GetStringAsync("/swagger/v1/swagger.json");
                Approvals.Verify(result);

                var schemaFile = "chatroom_client_swagger_schema.json";
                var schemaFilePath = Path.Join("Schema", schemaFile);
                var schemaJson = File.ReadAllText(schemaFilePath);
                var schema = JObject.Parse(schemaJson).ToString();
                var resultJson = JObject.Parse(result).ToString();
                resultJson.Should().BeEquivalentTo(schema);
            }
        }
    }

    [Fact]
    public async Task ItCreateAndRemoveChannelTestAsync()
    {
        var observerMock = Mock.Of<ConsoleRoomObserver>();
        var controller = new ChatRoomClientController(_cluster.Client, _clientContext, observerMock, observerMock);

        // create nameof(ChatRoomClientControllerTests) channel
        await controller.CreateChannel(new CreateChannelRequest(nameof(ChatRoomClientControllerTests)));

        var channelResponse = await controller.GetChannels();
        var channels = (channelResponse.Result as OkObjectResult)?.Value as IEnumerable<ChannelInfo>;

        channels.Should().NotBeNull();
        channels!.Any(c => c.Name == nameof(ChatRoomClientControllerTests)).Should().BeTrue();

        // delete nameof(ChatRoomClientControllerTests) channel
        await controller.DeleteChannel(new DeleteChannelRequest(nameof(ChatRoomClientControllerTests)));

        channelResponse = await controller.GetChannels();
        channels = (channelResponse.Result as OkObjectResult)?.Value as IEnumerable<ChannelInfo>;

        channels.Should().NotBeNull();
        channels!.Any(c => c.Name == nameof(ChatRoomClientControllerTests)).Should().BeFalse();
    }

    [Fact]
    public async Task ItAddAndRemoveAgentToChannelTestAsync()
    {
        var observerMock = Mock.Of<ConsoleRoomObserver>();
        var observerRef = _cluster.Client.CreateObjectReference<IRoomObserver>(observerMock);
        var controller = new ChatRoomClientController(_cluster.Client, _clientContext, observerMock, observerMock);
        var roomGrain = _cluster.Client.GetGrain<IRoomGrain>(nameof(ChatRoomClientControllerTests));
        var testAgentName = "testAgent";
        await roomGrain.AddAgentToRoom(testAgentName, "test", true, observerRef);

        var members = await controller.GetRoomMembers();
        var membersList = (members.Result as OkObjectResult)?.Value as IEnumerable<AgentInfo>;
        membersList.Should().NotBeNull();
        membersList!.Any(m => m.Name == testAgentName).Should().BeTrue();

        // test add agent to channel
        var channelName = nameof(ChatRoomClientControllerTests);
        await controller.CreateChannel(new CreateChannelRequest(channelName));
        await controller.AddAgentToChannel(new AddAgentToChannelRequest(channelName, testAgentName));

        var channelMembers = await controller.GetChannelMembers(new GetChannelMembersRequest(channelName));
        var channelMembersList = (channelMembers.Result as OkObjectResult)?.Value as IEnumerable<AgentInfo>;
        channelMembersList.Should().NotBeNull();
        channelMembersList!.Any(m => m.Name == testAgentName).Should().BeTrue();

        // test remove agent from channel
        await controller.RemoveAgentFromChannel(new RemoveAgentFromChannelRequest(channelName, testAgentName));

        channelMembers = await controller.GetChannelMembers(new GetChannelMembersRequest(channelName));
        channelMembersList = (channelMembers.Result as OkObjectResult)?.Value as IEnumerable<AgentInfo>;
        channelMembersList.Should().NotBeNull();
        channelMembersList!.Any(m => m.Name == testAgentName).Should().BeFalse();

        // test remove agent from room
        await roomGrain.RemoveAgentFromRoom(testAgentName);

        members = await controller.GetRoomMembers();
        membersList = (members.Result as OkObjectResult)?.Value as IEnumerable<AgentInfo>;
        membersList.Should().NotBeNull();
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
                .ConfigureServices(services =>
                {
                    services.AddSingleton<ChannelConfiguration>();
                });
        }
    }

}
