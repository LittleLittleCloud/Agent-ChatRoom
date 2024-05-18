using ChatRoom;

public readonly record struct ClientContext(
    IClusterClient ChannelClient,
    string? UserName = null,
    string? CurrentChannel = null,
    string? CurrentRoom = null)
{
    /// <summary>
    /// Check if the client is connected to a channel.
    /// </summary>
    public bool IsConnectedToChannel => CurrentChannel is not null;

    public AgentInfo? AgentInfo { get => new AgentInfo(UserName!, "Human user", true); }
}
