using ChatRoom.Common;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Orleans.Streams;
using Orleans.Utilities;

namespace ChatRoom.Room;

public class RoomGrain : Grain, IRoomGrain
{
    private readonly ObserverManager<IRoomObserver> _roomObserver;
    private readonly List<AgentInfo> _members = new(100);
    private readonly List<ChannelInfo> _channels = new(100);
    private IAsyncStream<AgentInfo> _addAgentStream = null!;
    private IAsyncStream<AgentInfo> _removeAgentStream = null!;
    private IAsyncStream<ChannelInfo> _addChannelStream = null!;
    private IAsyncStream<ChannelInfo> _removeChannelStream = null!;

    public RoomGrain(ILogger<RoomGrain> logger)
    {
        _roomObserver = new ObserverManager<IRoomObserver>(TimeSpan.FromMinutes(1), logger);
    }
    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var streamProvider = this.GetStreamProvider("chat");
        var addAgentStreamId = StreamId.Create("Room-AddAgent", this.GetPrimaryKeyString());
        var removeAgentStreamId = StreamId.Create("Room-RemoveAgent", this.GetPrimaryKeyString());
        _addAgentStream = streamProvider.GetStream<AgentInfo>(addAgentStreamId);
        _removeAgentStream = streamProvider.GetStream<AgentInfo>(removeAgentStreamId);

        var addChannelStreamId = StreamId.Create("Room-AddChannel", this.GetPrimaryKeyString());
        var removeChannelStreamId = StreamId.Create("Room-RemoveChannel", this.GetPrimaryKeyString());
        _addChannelStream = streamProvider.GetStream<ChannelInfo>(addChannelStreamId);
        _removeChannelStream = streamProvider.GetStream<ChannelInfo>(removeChannelStreamId);

        await base.OnActivateAsync(cancellationToken);
    }

    public Task<AgentInfo[]> GetMembers() => Task.FromResult(_members.ToArray());

    public async Task<StreamId> Join(AgentInfo nickname)
    {
        if (_members.Any(x => x.Name == nickname.Name))
        {
            return _addAgentStream.StreamId;
        }

        _members.Add(nickname);
        await _addAgentStream.OnNextAsync(nickname);
        var agentJoinMessage = new ChatMsg("System", $"{nickname.Name} joins the chat room.");
        await _roomObserver.Notify(x => x.Notification(agentJoinMessage));
        await _roomObserver.Notify(x => x.Join(nickname));

        return _addAgentStream.StreamId;
    }

    public async Task<StreamId> Leave(string nickname)
    {
        var agentInfo = _members.FirstOrDefault(x => x.Name == nickname);
        if (agentInfo is null)
        {
            return _removeAgentStream.StreamId;
        }

        _members.Remove(agentInfo);
        await _removeAgentStream.OnNextAsync(agentInfo);
        var agentLeaveMessage = new ChatMsg("System", $"{nickname} leaves the chat room.");
        await _roomObserver.Notify(x => x.Notification(agentLeaveMessage));
        await _roomObserver.Notify(x => x.Leave(agentInfo));

        return _removeAgentStream.StreamId;
    }

    public Task<ChannelInfo[]> GetChannels()
    {
        return Task.FromResult(_channels.ToArray());
    }

    public async Task CreateChannel(ChannelInfo channelInfo)
    {
        if (_channels.Any(x => x.Name == channelInfo.Name))
        {
            return;
        }

        await _addChannelStream.OnNextAsync(channelInfo);

        _channels.Add(channelInfo);
    }

    public async Task DeleteChannel(string channelName)
    {
        if (_channels.All(x => x.Name != channelName))
        {
            return;
        }

        var channel = _channels.First(x => x.Name == channelName);

        await _removeChannelStream.OnNextAsync(channel);
    }

    public Task Subscribe(IRoomObserver observer)
    {
        _roomObserver.Subscribe(observer, observer);

        return Task.CompletedTask;
    }

    public Task Unsubscribe(IRoomObserver observer)
    {
        _roomObserver.Unsubscribe(observer);

        return Task.CompletedTask;
    }

    public async Task AddAgentToChannel(ChannelInfo channelInfo, AgentInfo agentInfo)
    {
        if (_channels.All(x => x.Name != channelInfo.Name))
        {
            var channelNotFoundMessage = new ChatMsg("System", $"Channel '{channelInfo.Name}' not found.");
            await _roomObserver.Notify(x => x.Notification(channelNotFoundMessage));
        }

        var channel = _channels.First(x => x.Name == channelInfo.Name);

        if (_members.All(x => x.Name != agentInfo.Name))
        {
            var agentNotFoundMessage = new ChatMsg("System", $"Agent '{agentInfo.Name}' not found.");
            await _roomObserver.Notify(x => x.Notification(agentNotFoundMessage));
        }

        var agent = _members.First(x => x.Name == agentInfo.Name);

        await _roomObserver.Notify(x => x.AddMemberToChannel(channel, agent));
    }

    public async Task RemoveAgentFromChannel(ChannelInfo channelInfo, AgentInfo agentInfo)
    {
        if (_channels.All(x => x.Name != channelInfo.Name))
        {
            var channelNotFoundMessage = new ChatMsg("System", $"Channel '{channelInfo.Name}' not found.");
            await _roomObserver.Notify(x => x.Notification(channelNotFoundMessage));
        }

        var channel = _channels.First(x => x.Name == channelInfo.Name);

        if (_members.All(x => x.Name != agentInfo.Name))
        {
            var agentNotFoundMessage = new ChatMsg("System", $"Agent '{agentInfo.Name}' not found.");
            await _roomObserver.Notify(x => x.Notification(agentNotFoundMessage));
        }

        var agent = _members.First(x => x.Name == agentInfo.Name);

        await _roomObserver.Notify(x => x.RemoveMemberFromChannel(channel, agent));
    }
}
