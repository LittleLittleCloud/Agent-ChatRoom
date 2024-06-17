using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChatRoom.Client.DTO;
using ChatRoom.SDK;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System.IO;

namespace ChatRoom.Client;

[ApiController]
[Route("api/[controller]/[action]")]
public class ChatRoomClientController : Controller
{
    private readonly IClusterClient _clusterClient = null!;
    private readonly ClientContext _clientContext = null!;
    private readonly ILogger<ChatRoomClientController>? _logger = null!;
    private readonly IRoomObserver _roomObserverRef = null!;
    private readonly ConsoleRoomObserver _consoleRoomObserver = null!;
    private readonly ChatPlatformClient _chatPlatformClient = null!;
    private readonly ChatRoomClientConfiguration _config = null!;

    public ChatRoomClientController(
        IClusterClient clusterClient,
        ClientContext clientContext,
        IRoomObserver roomObserverRef,
        ConsoleRoomObserver consoleRoomObserver,
        ChatPlatformClient? chatPlatformClient = null!,
        ChatRoomClientConfiguration? config = null,
        ILogger<ChatRoomClientController>? logger = null)
    {
        _clusterClient = clusterClient;
        _clientContext = clientContext;
        _logger = logger;
        _roomObserverRef = roomObserverRef;
        _consoleRoomObserver = consoleRoomObserver;
        _config = config ?? new ChatRoomClientConfiguration();
        _chatPlatformClient = chatPlatformClient ?? new ChatPlatformClient(_clusterClient, _config.RoomConfig.Room);
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

        await channelGrain.SendMessage(message);

        return new OkResult();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<string>>> GetRoomCheckpoints()
    {
        _logger?.LogInformation("Getting checkpoints");
        var checkpoints = await ListCheckpoints();

        var checkpointsList = checkpoints.Select(x => System.IO.Path.GetFileName(x));
        return new OkObjectResult(checkpointsList);
    }

    [HttpGet]
    public async Task<ActionResult> UnloadCheckpoint()
    {

       _logger?.LogInformation("Unloading checkpoint");
        var room = _clientContext.CurrentRoom;
        var roomGrain = _clusterClient.GetGrain<IRoomGrain>(room);
        // remove all channels
        var channels = await roomGrain.GetChannels();
        foreach (var channel in channels)
        {
            await roomGrain.DeleteChannel(channel.Name);
        }

        return new OkResult();
    }

    [HttpGet]
    public async Task<ActionResult> LoadCheckpoint(string checkpointName)
    {
        _logger?.LogInformation("Loading checkpoint {checkpoint}", checkpointName);
        var checkpoints = await ListCheckpoints();
        var checkpointPath = checkpoints.FirstOrDefault(x => System.IO.Path.GetFileName(x) == checkpointName);

        if (checkpointPath == null)
        {
            return new BadRequestObjectResult("Checkpoint does not exist");
        }
        var room = _clientContext.CurrentRoom;
        var roomGrain = _clusterClient.GetGrain<IRoomGrain>(room);
        // remove all channels
        var channels = await roomGrain.GetChannels();
        foreach (var channel in channels)
        {
            await roomGrain.DeleteChannel(channel.Name);
        }

        var schema = JsonSerializer.Deserialize<ChatRoomContextSchemaV0>(System.IO.File.ReadAllText(checkpointPath))!;
        var workspaceConfiguration = ChatRoomContext.FromSchema(schema);
        foreach (var channel in workspaceConfiguration.Channels)
        {
            var channelName = channel.Key;
            var channelMembers = channel.Value;
            var channelHistory = workspaceConfiguration.ChatHistory.TryGetValue(channelName, out var history) ? history : null;
            await roomGrain.CreateChannel(channelName, channelMembers, channelHistory);
            _logger?.LogInformation("Restored channel {ChannelName} with {MemberCount} members and {HistoryCount} history items", channelName, channelMembers.Count(), channelHistory?.Count() ?? 0);
        }

        return new OkResult();
    }

    [HttpGet]
    public async Task<ActionResult> SaveCheckpoint()
    {
        _logger?.LogInformation("Saving checkpoint");
        var room = _clientContext.CurrentRoom;
        var roomGrain = _clusterClient.GetGrain<IRoomGrain>(room);
        var channels = await roomGrain.GetChannels();
        Dictionary<string, ChatMsg[]> chatHistory = new();
        Dictionary<string, string[]> channelMembers = new();
        foreach (var channel in channels)
        {
            var channelGrain = _clusterClient.GetGrain<IChannelGrain>(channel.Name);
            var history = await channelGrain.ReadHistory(100);
            chatHistory[channel.Name] = history.ToArray();
            var members = await channelGrain.GetMembers();
            channelMembers[channel.Name] = members.Select(m => m.Name).ToArray();
        }

        var workspaceConfiguration = new ChatRoomContext
        {
            Channels = channelMembers,
            ChatHistory = chatHistory,
        };

        var schema = workspaceConfiguration.ToSchema();
        var json = JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });

        var workspace = _config.Workspace;
        var checkpointDir = Path.Combine(workspace, "checkpoints");
        if (!Directory.Exists(checkpointDir))
        {
            Directory.CreateDirectory(checkpointDir);
        }

        var dateTimeNow = DateTime.Now;

        var checkpointPath = Path.Combine(checkpointDir, $"{room}_{dateTimeNow:yyyy-MM-dd_HH-mm-ss}.json");

        await System.IO.File.WriteAllTextAsync(checkpointPath, json);

        return new OkResult();
    }

    [HttpGet]
    [Route("{checkpointPath}")]
    public async Task<ActionResult> DeleteCheckpoint(string checkpointPath)
    {
        _logger?.LogInformation("Deleting checkpoint {checkpoint}", checkpointPath);
        var workspace = _config.Workspace;
        var checkpointDir = Path.Combine(workspace, "checkpoints");
        checkpointPath = Path.Combine(checkpointDir, checkpointPath);
        if (Path.Exists(checkpointPath))
        {
            System.IO.File.Delete(checkpointPath);
        }

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
    public async Task<ActionResult<AgentInfo>> GetUserInfo()
    {
        _logger?.LogInformation("Getting user info");
        return new OkObjectResult(new AgentInfo(_clientContext.UserName!, _clientContext.Description!));
    }

    [HttpGet]
    [Route("{channelName}")]
    public async Task<ActionResult> ClearHistory(string channelName)
    {
        _logger?.LogInformation("Deleting messages in channel {channelName}", channelName);

        var channelGrain = _clusterClient.GetGrain<IChannelGrain>(channelName);
        await channelGrain.ClearHistory();

        return new OkResult();
    }

    [HttpGet]
    [Route("{channelName}")]
    public async Task NewMessageSse(string channelName)
    {
        var channelGrain = _clusterClient.GetGrain<IChannelGrain>(channelName);
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");

        EventHandler<ChatMsg> handler = (sender, msg) =>
        {
            var sseEvent = new SSEEvent
            {
                Id = msg.Created.ToString(),
                Retry = 1000,
                Event = "message",
                Data = JsonSerializer.Serialize(msg),
            };

            var sseData = Encoding.UTF8.GetBytes(sseEvent.ToString());
            Response.Body.WriteAsync(sseData);
            Response.Body.WriteAsync(Encoding.UTF8.GetBytes("\n\n"));
            Response.Body.FlushAsync();
        };

        _consoleRoomObserver.OnMessageReceived += handler;
        
        while (!HttpContext.RequestAborted.IsCancellationRequested)
        {
            await Task.Delay(1000);
        }

        _consoleRoomObserver.OnMessageReceived -= handler;
    }

    [HttpPost]
    public async Task<ActionResult<GenerateNextReplyResponse>> GenerateNextReply(
        [FromBody] GenerateNextReplyRequest request)
    {
        _logger?.LogInformation("Generating next reply");

        var channelName = request.ChannelName;
        var chatMsgs = request.ChatMsgs;
        var candidates = request.Candidates;

        var channelGrain = _clusterClient.GetGrain<IChannelGrain>(channelName);
        var reply = await channelGrain.GenerateNextReply(candidates, chatMsgs, orchestrator: request.Orchestrator);

        return new OkObjectResult(new GenerateNextReplyResponse(reply));
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
    [Route("{channelName}")]
    public async Task<ActionResult<ChannelInfo>> GetChannelInfo(string channelName)
    {
        _logger?.LogInformation("Getting info of channel {channelName}", channelName);

        var channelGrain = _clusterClient.GetGrain<IChannelGrain>(channelName);
        var info = await channelGrain.GetChannelInfo();

        return new OkObjectResult(info);
    }

    [HttpPost]
    public async Task<ActionResult> EditTextMessage(
        [FromBody] EditTextMessageRequest request)
    {
        var channelName = request.ChannelName;
        var messageId = request.MessageId;
        var newText = request.NewText;

        _logger?.LogInformation("Editing message {messageId} in channel {channelName}", messageId, channelName);

        var channelGrain = _clusterClient.GetGrain<IChannelGrain>(channelName);
        await channelGrain.EditTextMessage(messageId, newText);

        return new OkResult();
    }

    [HttpGet]
    [Route("{channelName}/{messageId}")]
    public async Task<ActionResult> DeleteMessage(string channelName, long messageId)
    {
        _logger?.LogInformation("Deleting message {messageId} in channel {channelName}", messageId, channelName);

        var channelGrain = _clusterClient.GetGrain<IChannelGrain>(channelName);
        await channelGrain.DeleteMessage(messageId);

        return new OkResult();
    }


    [HttpPost]
    public async Task<ActionResult<IEnumerable<AgentInfo>>> GetChannelMembers(
        [FromBody] GetChannelMembersRequest request)
    {
        var channelName = request.ChannelName;
        _logger?.LogInformation("Getting members of channel {channelName}", channelName);

        var channelGrain = _clusterClient.GetGrain<IChannelGrain>(channelName);
        var members = await channelGrain.GetMembers();

        return new OkObjectResult(members);
    }

    [HttpPost]
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
        await channelGrain.AddAgentToChannel(_clientContext.UserName!, _clientContext.Description!, true, _roomObserverRef);

        return new OkResult();
    }

    [HttpPost]
    public async Task<ActionResult> LeaveChannel(
        [FromBody] LeaveChannelRequest request)
    {
        var channelName = request.ChannelName;
        _logger?.LogInformation("Leaving channel {channelName}", channelName);

        var channelGrain = _clusterClient.GetGrain<IChannelGrain>(channelName);
        await channelGrain.RemoveAgentFromChannel(_clientContext.UserName!);

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
        await roomGrain.AddAgentToChannel(channelName, agentName);

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
        await roomGrain.RemoveAgentFromChannel(channelName, agentName);

        return new OkResult();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<string>>> GetOrchestrators()
    {
        _logger?.LogInformation("Getting orchestrators");
        var roomGrain = _clusterClient.GetGrain<IRoomGrain>(_clientContext.CurrentRoom);
        var orchestrators = await roomGrain.GetOrchestrators();

        return new OkObjectResult(orchestrators);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<string>>> GetChannelOrchestrators(string channel)
    {
        _logger?.LogInformation("Getting orchestrators of channel");

        var orchestrators = await _chatPlatformClient.GetOrchestrators();

        return new OkObjectResult(orchestrators);
    }

    [HttpPost]
    public async Task<ActionResult> AddOrchestratorToChannel(
        [FromBody] AddOrchestratorToChannelRequest request)
    {
        var channelName = request.ChannelName;
        var orchestratorName = request.OrchestratorName;
        _logger?.LogInformation("Adding orchestrator {orchestratorName} to channel {channelName}", orchestratorName, channelName);
        
        await _chatPlatformClient.AddOrchestratorToChannel(channelName, orchestratorName);

        return new OkResult();
    }

    [HttpPost]
    public async Task<ActionResult> RemoveOrchestratorFromChannel(
        [FromBody] RemoveOrchestratorFromChannelRequest request)
    {
        var channelName = request.ChannelName;
        var orchestratorName = request.OrchestratorName;
        _logger?.LogInformation("Removing orchestrator {orchestratorName} from channel {channelName}", orchestratorName, channelName);

        await _chatPlatformClient.RemoveOrchestratorFromChannel(channelName, orchestratorName);
        return new OkResult();
    }

    private Task<IOrderedEnumerable<string>> ListCheckpoints()
    {
        var workspace = _config.Workspace;
        var checkpointDir = Path.Combine(workspace, "checkpoints");

        if (!Directory.Exists(checkpointDir))
        {
            return Task.FromResult(Enumerable.Empty<string>().OrderByDescending(x => x));
        }

        // checkpoints are a list of json files that in the format of {roomName}_{YYYY-MM-DD_HH-mm-ss}.json
        var roomName = _config.RoomConfig.Room;
        var re = new Regex($@"^{roomName}_(\d{{4}}-\d{{2}}-\d{{2}}_\d{{2}}-\d{{2}}-\d{{2}})\.json$");
        var checkpoints = Directory.GetFiles(checkpointDir, "*.json")
            .Where(x => re.IsMatch(System.IO.Path.GetFileName(x)))
            .OrderByDescending(x => x);

        return Task.FromResult(checkpoints);
    }
}

public struct SSEEvent
{
    public string Id { get; set; }
    public int Retry { get; set; }
    public string Event { get; set; }
    public string Data { get; set; }

    public override string ToString()
    {
        return $"id: {Id}\nretry: {Retry}\nevent: {Event}\ndata: {Data}\n\n";
    }
}
