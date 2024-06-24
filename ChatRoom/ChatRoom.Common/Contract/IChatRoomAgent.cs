using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans.Concurrency;

namespace ChatRoom.SDK;

public interface IChatRoomAgent
{
    [OneWay]
    Task Notification(ChatMsg msg);

    [OneWay]
    Task JoinRoom(AgentInfo agent, string room);

    [OneWay]
    Task LeaveRoom(AgentInfo agent, string room);

    [OneWay]
    Task JoinChannel(AgentInfo agent, ChannelInfo channelInfo);

    [OneWay]
    Task LeaveChannel(AgentInfo agent, ChannelInfo channelInfo);

    Task<ChatMsg?> GenerateReplyAsync(AgentInfo agent, ChatMsg[] msg, ChannelInfo channelInfo);

    [OneWay]
    Task NewMessage(ChatMsg msg);
}
