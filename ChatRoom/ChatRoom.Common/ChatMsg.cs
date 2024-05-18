using AutoGen.Core;

namespace ChatRoom;

[GenerateSerializer]
public record class ChatMsg(
    string? From,
    string Text) : IMessage
{
    [Id(0)]
    public string From { get; set; } = From ?? "Alexey";

    [Id(1)]
    public DateTimeOffset Created { get; init; } = DateTimeOffset.Now;
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
}
