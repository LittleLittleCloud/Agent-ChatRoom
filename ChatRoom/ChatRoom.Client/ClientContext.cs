using ChatRoom;

public class ClientContext
{
    public string? CurrentChannel { get; set; }

    public string? CurrentRoom { get; set; }

    public string? UserName { get; set; }

    public string? Description { get; set; } = "Human user";

    /// <summary>
    /// Check if the client is connected to a channel.
    /// </summary>
    public bool IsConnectedToChannel => CurrentChannel is not null;
}
