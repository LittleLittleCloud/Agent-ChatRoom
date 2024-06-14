using AutoGen.Core;

namespace ChatRoom;

/// <summary>
/// Must contain either TextPart or [ImageData, MimeType] or [ImageUrl, MimeType].
/// </summary>
[GenerateSerializer]
public record class ChatMsgPart
{
    [Id(0)]
    public string? TextPart { get; set; }

    [Id(1)]
    public byte[]? ImageData { get; set; }

    [Id(2)]
    public Uri? ImageUrl { get; set; }

    [Id(3)]
    public string? MimeType { get; set; }
}

[GenerateSerializer]
[Alias("ChatMsg")]
public record class ChatMsg : IMessage, ICanGetTextContent
{
    public ChatMsg(string? From, string Text)
    {
        this.From = From;
        this.Parts = [new() { TextPart = Text }];
    }

    [Id(0)]
    public string? From { get; set; } = "Alexey";

    [Id(1)]
    public DateTimeOffset Created { get; init; } = DateTimeOffset.Now;

    [Id(2)]
    public ChatMsgPart[] Parts { get; init; } = [];

    /// <summary>
    /// ID is the timestamp in seconds of the message creation.
    /// </summary>
    [Id(3)]
    public long ID { get; init; } = DateTimeOffset.Now.ToUnixTimeSeconds();

    public string? GetContent()
    {
        if (Parts is { Length: 1} && Parts[0].TextPart is not null)
        {
            return Parts[0].TextPart;
        }

        return null;
    }

    public IMessage ToAutoGenMessage()
    {
        var messages = new List<IMessage>();

        foreach (var part in this.Parts)
        {
            if (part.TextPart is string content)
            {
                messages.Add(new TextMessage(Role.Assistant, content, from: this.From));
            }

            if (part.ImageData is not null)
            {
                messages.Add(new ImageMessage(Role.Assistant, BinaryData.FromBytes(part.ImageData)!, from: this.From));
            }

            if (part.ImageUrl is not null)
            {
                messages.Add(new ImageMessage(Role.Assistant, part.ImageUrl!, from: this.From));
            }
        }

        if (messages.Count == 1)
        {
            return messages[0];
        }
        else
        {
            return new MultiModalMessage(Role.Assistant, messages.ToArray(), from: this.From);
        }
    }
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
