namespace KvsCache.Models.Azure;

public class KeyVault : DataChunk
{
    public KeyVault(string? id, string name, string url)
    {
        Id = id;
        Name = name;
        Url = url;
    }

    public string? Id { get; }
    public string Name { get; }
    public string Url { get; }

    public override string DisplayName => Name;
}
