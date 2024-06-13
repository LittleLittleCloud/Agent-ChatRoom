using AutoGen.Core;

namespace ChatRoom;

[GenerateSerializer]
public record class ChatMsg : IMessage
{
    public ChatMsg(string? From, string Text)
    {
        this.From = From;
        this.Text = Text;
    }

    [Id(0)]
    public string? From { get; set; } = "Alexey";

    [Id(1)]
    public DateTimeOffset Created { get; init; } = DateTimeOffset.Now;

    [Id(2)]
    public string Text { get; }

    /// <summary>
    /// ID is the timestamp in seconds of the message creation.
    /// </summary>
    [Id(3)]
    public long ID { get; init; } = DateTimeOffset.Now.ToUnixTimeSeconds();
}

[GenerateSerializer]
public record class ChannelInfo
{
    public ChannelInfo(string name)
    {
        Name = name;
    }

    [Id(0)]
    public string Name { get; init; }

    [Id(1)]
    public List<AgentInfo> Members { get; init; } = new();
}
