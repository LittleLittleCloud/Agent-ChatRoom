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
