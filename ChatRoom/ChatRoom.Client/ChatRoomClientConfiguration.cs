﻿using System.Text.Json.Serialization;
using ChatRoom.Room;
using Json.Schema.Generation;

namespace ChatRoom.Client;

public class ChatRoomClientConfiguration
{
    [Description("The configuration for the chat room")]
    [JsonPropertyName("room_config")]
    public RoomConfiguration RoomConfig { get; set; } = new RoomConfiguration();

    [Description("The configuration for the chat channel")]
    [JsonPropertyName("channel_config")]
    public ChannelConfiguration ChannelConfig { get; set; } = new ChannelConfiguration();

    [JsonPropertyName("agent_extensions")]
    public List<AgentExtensionConfiguration> AgentExtensions { get; set; } = [];

    [JsonPropertyName("name")]
    [Description("Your name in the chat room")]
    public string YourName { get; set; } = "User";
}

public abstract class AgentExtensionConfiguration
{
    [Description("The name of the extension")]
    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;

    [Description("The command to start the agent extension")]
    [JsonPropertyName("command")]
    public string Command { get; init; } = null!;
}
