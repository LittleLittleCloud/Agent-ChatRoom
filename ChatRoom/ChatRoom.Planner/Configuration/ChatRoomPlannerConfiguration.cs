using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ChatRoom.SDK;
using Json.Schema.Generation;

namespace ChatRoom.Planner;

internal class ChatRoomPlannerConfiguration
{
    [JsonPropertyName("room_config")]
    [Description("the configuration for chat room")]
    public RoomConfiguration RoomConfig { get; set; } = new RoomConfiguration();

    [Description("the configuration for react planner")]
    [JsonPropertyName("react_planner_configuration")]
    public ReActPlannerConfiguration ReActPlannerConfiguration { get; set; } = new ReActPlannerConfiguration();
}
