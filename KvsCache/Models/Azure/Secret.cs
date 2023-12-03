namespace KvsCache.Models.Azure;

public class Secret : DataItem
{
    public Secret(string id, string name)
    {
        Id = id;
        Name = name;
    }

    public string Id { get; }
    public string Name { get; }

    public override string DisplayName => Name;
}

