//using AutoGen.Core;

//namespace ChatRoom.Common;

//public class RoomObserver : IRoomObserver
//{
//    private readonly IAgent _agent;
//    private readonly IClusterClient _client;
//    private readonly IDictionary<string, IChannelObserver> _channelObservers = new Dictionary<string, IChannelObserver>();

//    public RoomObserver(IAgent agent, IClusterClient client)
//    {
//        _agent = agent;
//        _client = client;
//    }

//    //public async Task AddMemberToChannel(ChannelInfo channel, AgentInfo agent)
//    //{
//    //    if (agent.Name == _agent.Name)
//    //    {
//    //        var channelGrain = _client.GetGrain<IChannelGrain>(channel.Name);
//    //        var channelObserver = new ChannelObserver(_agent, channelGrain);
//    //        var reference = _client.CreateObjectReference<IChannelObserver>(channelObserver);
//    //        _channelObservers[channel.Name] = reference;
//    //        await channelGrain.Join(agent);
//    //        await channelGrain.Subscribe(reference);
//    //    }
//    //}

//    //public async Task RemoveMemberFromChannel(ChannelInfo channel, AgentInfo agent)
//    //{
//    //    if (agent.Name == _agent.Name)
//    //    {
//    //        var channelGrain = _client.GetGrain<IChannelGrain>(channel.Name);
//    //        var channelObserver = _channelObservers[channel.Name];
//    //        await channelGrain.Unsubscribe(channelObserver);
//    //        await channelGrain.Leave(agent);
//    //        _channelObservers.Remove(channel.Name);
//    //    }
//    //}

//    public Task Notification(ChatMsg msg)
//    {
//        Console.WriteLine($"Message from {msg.From}: {msg.Text}");
//        return Task.CompletedTask;
//    }

//    public Task Join(AgentInfo agent)
//    {
//        Console.WriteLine($"Agent {agent.Name} joined the chat room");

//        return Task.CompletedTask;
//    }

//    public Task Leave(AgentInfo agent)
//    {
//        Console.WriteLine($"Agent {agent.Name} left the chat room");

//        return Task.CompletedTask;
//    }

//    public Task<bool> Ping() => Task.FromResult(true);
//    public Task<ChatMsg?> GenerateReplyAsync(AgentInfo agent, ChatMsg[] msg) => throw new NotImplementedException();
//    public Task NewMessage(ChatMsg msg) => throw new NotImplementedException();
//}
