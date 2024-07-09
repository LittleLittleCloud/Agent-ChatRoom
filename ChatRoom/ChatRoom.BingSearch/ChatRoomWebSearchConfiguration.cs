using ChatRoom.SDK;
using Json.Schema.Generation;
using System.Text.Json.Serialization;

namespace ChatRoom.WebSearch;

internal class ChatRoomWebSearchConfiguration
{
    [JsonPropertyName("bing_search_config")]
    [Description("The configuration for Bing search agent")]
    public BingSearchConfiguration? BingSearchConfiguration { get; set; }

    [JsonPropertyName("google_search_config")]
    [Description("The configuration for Google search agent")]
    public GoogleSearchConfiguration? GoogleSearchConfiguration { get; set; }

    [JsonPropertyName("room_config")]
    [Description("The configuration for the chat room")]
    public RoomConfiguration RoomConfig { get; set; } = new RoomConfiguration();
}

