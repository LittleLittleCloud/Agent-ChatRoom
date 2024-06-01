using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatRoom.Client.DTO;
using ChatRoom.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ChatRoom.Client;

[ApiController]
[Route("api/[controller]/[action]")]
public class ChatRoomClientController
{
    private readonly IClusterClient _clusterClient = null!;
    private readonly ClientContext _clientContext = null!;
    private readonly ILogger<ChatRoomClientController>? _logger = null!;
    private readonly IRoomObserver _roomObserverRef = null!;

    public ChatRoomClientController(
        IClusterClient clusterClient,
        ClientContext clientContext,
        IRoomObserver roomObserverRef,
        ILogger<ChatRoomClientController>? logger = null)
    {
        _clusterClient = clusterClient;
        _clientContext = clientContext;
        _logger = logger;
        _roomObserverRef = roomObserverRef;
    }

    [HttpPost]
    public async Task<ActionResult> SendTextMessageToChannel(
        [FromBody] SendTextMessageToChannelRequest request)
    {
        var message = request.Message;
        var channel = request.ChannelName;

        // if message.from != _clientContext.User
        // return BadRequest("You are not authorized to send message to this channel");
        if (message == null || string.IsNullOrWhiteSpace(channel))
        {
            return new BadRequestResult();
        }

        if (message.From != _clientContext.UserName)
        {
            return new BadRequestObjectResult("You are not authorized to send message to this channel");
        }

        var channelGrain = _clusterClient.GetGrain<IChannelGrain>(channel);

        await channelGrain.Message(message);

        return new OkResult();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChannelInfo>>> GetChannels()
    {
        _logger?.LogInformation("Getting channels");
        if (_clientContext.CurrentRoom == null)
        {
            return new BadRequestObjectResult("You are not in a room");
        }

        var roomGrain = _clusterClient.GetGrain<IRoomGrain>(_clientContext.CurrentRoom);
        var channels = await roomGrain.GetChannels();
        return new OkObjectResult(channels);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AgentInfo>>> GetRoomMembers()
    {
        _logger?.LogInformation("Getting members");
        if (_clientContext.CurrentRoom == null)
        {
            return new BadRequestObjectResult("You are not in a room");
        }

        var roomGrain = _clusterClient.GetGrain<IRoomGrain>(_clientContext.CurrentRoom);
        var members = await roomGrain.GetMembers();
        return new OkObjectResult(members);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AgentInfo>>> GetChannelMembers(
        [FromBody] GetChannelMembersRequest request)
    {
        var channelName = request.ChannelName;
        _logger?.LogInformation("Getting members of channel {channelName}", channelName);

        var channelGrain = _clusterClient.GetGrain<IChannelGrain>(channelName);
        var members = await channelGrain.GetMembers();

        return new OkObjectResult(members);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChatMsg>>> GetChannelChatHistory(
        [FromBody] GetChannelChatHistoryRequest request)
    {
        var channelName = request.ChannelName;
        if (request.Count <= 0)
        {
            return new BadRequestObjectResult("Count must be greater than 0");
        }

        _logger?.LogInformation("Getting history of channel {channelName}", channelName);

        var channelGrain = _clusterClient.GetGrain<IChannelGrain>(channelName);
        var history = await channelGrain.ReadHistory(request.Count);

        return new OkObjectResult(history);
    }

    [HttpPost]
    public async Task<ActionResult> CreateChannel(
        [FromBody] CreateChannelRequest request)
    {
        var channelName = request.ChannelName;
        _logger?.LogInformation("Creating channel {channelName}", channelName);

        var roomGrain = _clusterClient.GetGrain<IRoomGrain>(_clientContext.CurrentRoom);
        await roomGrain.CreateChannel(channelName);

        return new OkResult();
    }

    [HttpPost]
    public async Task<ActionResult> JoinChannel(
        [FromBody] JoinChannelRequest request)
    {
        var channelName = request.ChannelName;
        _logger?.LogInformation("Joining channel {channelName}", channelName);
        var channelResponse = await GetChannels();
        var channels = (channelResponse.Result as OkObjectResult)?.Value as IEnumerable<ChannelInfo>;
        if (channels?.All(x => x.Name != channelName) is true)
        {
            if (request.CreateIfNotExists)
            {
                _logger?.LogInformation("Channel {channelName} does not exist, creating it", channelName);
                await CreateChannel(new CreateChannelRequest(channelName));
            }
            else
            {
                _logger?.LogWarning("Channel {channelName} does not exist", channelName);
                return new BadRequestObjectResult("Channel does not exist");
            }
        }

        var channelGrain = _clusterClient.GetGrain<IChannelGrain>(channelName);
        await channelGrain.JoinChannel(_clientContext.UserName!, _clientContext.Description!, true, _roomObserverRef);

        return new OkResult();
    }

    [HttpPost]
    public async Task<ActionResult> LeaveChannel(
        [FromBody] LeaveChannelRequest request)
    {
        var channelName = request.ChannelName;
        _logger?.LogInformation("Leaving channel {channelName}", channelName);

        var channelGrain = _clusterClient.GetGrain<IChannelGrain>(channelName);
        await channelGrain.LeaveChannel(_clientContext.UserName!);

        return new OkResult();
    }

    [HttpPost]
    public async Task<ActionResult> DeleteChannel(
        [FromBody] DeleteChannelRequest request)
    {
        var channelName = request.ChannelName;
        _logger?.LogInformation("Deleting channel {channelName}", channelName);

        var roomGrain = _clusterClient.GetGrain<IRoomGrain>(_clientContext.CurrentRoom);
        await roomGrain.DeleteChannel(channelName);

        return new OkResult();
    }

    [HttpPost]
    public async Task<ActionResult> AddAgentToChannel(
        [FromBody] AddAgentToChannelRequest request)
    {
        var channelName = request.ChannelName;
        var agentName = request.AgentName;
        _logger?.LogInformation("Adding agent {agentName} to channel {channelName}", agentName, channelName);

        var roomGrain = _clusterClient.GetGrain<IRoomGrain>(_clientContext.CurrentRoom);
        await roomGrain.AddAgentToChannel(new ChannelInfo(channelName), agentName);

        return new OkResult();
    }

    [HttpPost]
    public async Task<ActionResult> RemoveAgentFromChannel(
        [FromBody] RemoveAgentFromChannelRequest request)
    {
        var channelName = request.ChannelName;
        var agentName = request.AgentName;
        _logger?.LogInformation("Removing agent {agentName} from channel {channelName}", agentName, channelName);

        var roomGrain = _clusterClient.GetGrain<IRoomGrain>(_clientContext.CurrentRoom);
        await roomGrain.RemoveAgentFromChannel(new ChannelInfo(channelName), agentName);

        return new OkResult();
    }
}
