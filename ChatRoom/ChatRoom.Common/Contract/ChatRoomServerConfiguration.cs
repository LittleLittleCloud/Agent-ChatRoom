using System.Text.Json.Serialization;
using Json.Schema.Generation;

namespace ChatRoom.SDK;

public class ChatRoomServerConfiguration
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
    [Default("User")]
    public string YourName { get; set; } = "User";

    [JsonPropertyName("server_config")]
    [Description("The configuration for the server. If provided, the client will start a server for chatroom service")]
    public ServerConfiguration? ServerConfig { get; set; } = new ServerConfiguration();

    [JsonPropertyName("workspace")]
    [Description("The workspace to store logs, checkpoints and other files. The default value is the current directory.")]
    public string Workspace { get; set; } = Environment.CurrentDirectory;

    [JsonPropertyName("enable_console_app")]
    [Description("Enable the console app for the chat room")]
    [Default(false)]
    public bool EnableConsoleApp { get; set; } = false;
}

public class ServerConfiguration
{
    [Description("The urls to listen on")]
    [Default("http://localhost:51234;https://localhost:51235")]
    [JsonPropertyName("urls")]
    public string Urls { get; set; } = "http://localhost:51234;https://localhost:51235";

    [Description("environment, available values are Development, Staging, Production.")]
    [JsonPropertyName("environment")]
    [Default("Development")]
    public string Environment { get; set; } = "Development";
}

public class AgentExtensionConfiguration
{
    [Description("The name of the extension, can be any string")]
    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;

    [Description("The command to start the agent extension")]
    [JsonPropertyName("command")]
    public string Command { get; init; } = null!;
}
