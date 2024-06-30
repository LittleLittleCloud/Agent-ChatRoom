namespace ChatRoom.SDK;

internal interface IChatRoomAgentObserver : IChatRoomAgent, IGrainObserver
{
}

internal class ChatRoomAgentObserver : IChatRoomAgentObserver
{
    private readonly IChatRoomAgent _chatRoomAgent;

    public ChatRoomAgentObserver(IChatRoomAgent chatRoomAgent)
    {
        _chatRoomAgent = chatRoomAgent;
    }

    public Task Notification(ChatMsg msg)
    {
        return _chatRoomAgent.Notification(msg);
    }

    public Task JoinRoom(AgentInfo agent, string room)
    {
        return _chatRoomAgent.JoinRoom(agent, room);
    }

    public Task LeaveRoom(AgentInfo agent, string room)
    {
        return _chatRoomAgent.LeaveRoom(agent, room);
    }

    public Task JoinChannel(AgentInfo agent, ChannelInfo channelInfo)
    {
        return _chatRoomAgent.JoinChannel(agent, channelInfo);
    }

    public Task LeaveChannel(AgentInfo agent, ChannelInfo channelInfo)
    {
        return _chatRoomAgent.LeaveChannel(agent, channelInfo);
    }

    public Task<ChatMsg?> GenerateReplyAsync(AgentInfo agent, ChatMsg[] msg, ChannelInfo channelInfo)
    {
        return _chatRoomAgent.GenerateReplyAsync(agent, msg, channelInfo);
    }

    public Task NewMessage(ChatMsg msg)
    {
        return _chatRoomAgent.NewMessage(msg);
    }
}
