namespace ChatRoom;

[GenerateSerializer]
public record class AgentInfo
{
    public AgentInfo(string name, string description, bool isHuman = false)
    {
        Name = name;
        SelfDescription = description;
        IsHuman = isHuman;
    }

    [Id(0)]
    public string Name { get; init; }

    [Id(1)]
    public string SelfDescription { get; init; }

    [Id(2)]
    public bool IsHuman { get; init; } = false;

    public override string ToString()
    {
        if (SelfDescription is not null)
        {
            return $"{Name} - {SelfDescription}";
        }

        return Name;
    }

    public override int GetHashCode()
    {
        // name + description should be unique
        return Name.GetHashCode() ^ SelfDescription.GetHashCode();
    }
}
