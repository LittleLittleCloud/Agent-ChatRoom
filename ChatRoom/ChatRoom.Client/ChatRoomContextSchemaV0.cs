using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ChatRoom.Client;

public abstract class Schema
{
    [JsonPropertyName("type")]
    public abstract string SchemaType { get; }
}

public class ChatRoomContextSchemaV0 : Schema
{
    [JsonPropertyName("type")]
    public override string SchemaType => nameof(ChatRoomContextSchemaV0);

    [JsonPropertyName("channels")]
    public Dictionary<string, ChannelInfo> Channels { get; set; } = new();

    [JsonPropertyName("chat_history")]
    public Dictionary<string, ChatMsg[]> ChatHistory { get; set; } = new();

    [JsonPropertyName("current_channel")]
    public string CurrentChannel { get; set; } = "General";
}

public class ChatRoomContext
{
    public ChatRoomContext(ChatRoomContextSchemaV0 workspaceSchemaV0)
    {
        this.ChatHistory = workspaceSchemaV0.ChatHistory;
        this.Channels = workspaceSchemaV0.Channels;
        this.CurrentChannel = workspaceSchemaV0.CurrentChannel;
    }

    public ChatRoomContext()
    {
        this.ChatHistory = new();
        this.Channels = new();
        this.CurrentChannel = "General";
    }

    public Dictionary<string, ChannelInfo> Channels { get; set; }

    public Dictionary<string, ChatMsg[]> ChatHistory { get; set; }
    
    public string CurrentChannel { get; set; } = "General";

    public ChatRoomContextSchemaV0 ToSchema()
    {
        return new ChatRoomContextSchemaV0
        {
            Channels = this.Channels,
            ChatHistory = this.ChatHistory,
            CurrentChannel = this.CurrentChannel
        };
    }

    public static ChatRoomContext FromSchema(ChatRoomContextSchemaV0 schema)
    {
        return new ChatRoomContext(schema);
    }
}
