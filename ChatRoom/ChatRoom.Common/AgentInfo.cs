namespace ChatRoom;

[GenerateSerializer]
public record class AgentInfo
{
    public AgentInfo(string name, string? description)
    {
        Name = name;
        Description = description;
    }

    [Id(0)]
    public string Name { get; init; }

    [Id(1)]
    public string? Description { get; init; }

    [Id(2)]
    public bool IsHuman { get; init; } = false;

    public override string ToString()
    {
        if (Description is not null)
        {
            return $"{Name} - {Description}";
        }

        return Name;
    }
}
