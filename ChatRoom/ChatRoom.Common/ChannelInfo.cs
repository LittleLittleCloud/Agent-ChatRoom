namespace ChatRoom;

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

    [Id(2)]
    public List<string> Orchestrators { get; init; } = new();
}
