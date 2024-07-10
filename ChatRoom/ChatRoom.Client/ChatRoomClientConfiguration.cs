using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ChatRoom.SDK;
using ChatRoom.OpenAI;
using Json.Schema.Generation;
using ChatRoom.Github;
using ChatRoom.Powershell;
using ChatRoom.WebSearch;
using ChatRoom.Planner;

namespace ChatRoom.Client;

internal class ChatRoomClientConfiguration : ChatRoomServerConfiguration
{
    [JsonPropertyName("chatroom_openai_configuration")]
    [Description("ChatRoom OpenAI configuration, default is null")]
    public ChatRoomOpenAIConfiguration? ChatRoomOpenAIConfiguration { get; set; } = null;

    [JsonPropertyName("chatroom_github_configuration")]
    [Description("ChatRoom Github configuration, default is null")]
    public ChatRoomGithubConfiguration? ChatRoomGithubConfiguration { get; set; } = null;

    [JsonPropertyName("chatroom_powershell_configuration")]
    [Description("ChatRoom PowerShell configuration, default is null")]
    public ChatRoomPowershellConfiguration? ChatRoomPowershellConfiguration { get; set; } = null;

    [JsonPropertyName("chatroom_websearch_configuration")]
    [Description("ChatRoom WebSearch configuration, default is null")]
    public ChatRoomWebSearchConfiguration? ChatRoomWebSearchConfiguration { get; set; } = null;

    [JsonPropertyName("chatroom_planner_configuration")]
    [Description("ChatRoom Planner configuration, default is null")]
    public ChatRoomPlannerConfiguration? ChatRoomPlannerConfiguration { get; set; } = null;
}
