﻿using AutoGen.Core;

namespace ChatRoom;

[GenerateSerializer]
public record class ChatMsg : IMessage
{
    public ChatMsg(string From, string Text)
    {
        this.From = From;
        this.Text = Text;
    }

    [Id(0)]
    public string? From { get; set; } = "Alexey";

    [Id(1)]
    public DateTimeOffset Created { get; init; } = DateTimeOffset.Now;

    public string Text { get; }
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