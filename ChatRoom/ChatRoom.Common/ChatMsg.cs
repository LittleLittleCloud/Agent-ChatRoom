using AutoGen.Core;

namespace ChatRoom;

[GenerateSerializer]
public record class ChatMsg(
    string? Author,
    string Text) : IMessage
{
    [Id(0)]
    public string From { get; set; } = Author ?? "Alexey";

    [Id(1)]
    public DateTimeOffset Created { get; init; } = DateTimeOffset.Now;
}
