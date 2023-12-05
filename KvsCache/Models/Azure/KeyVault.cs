using Newtonsoft.Json;

namespace KvsCache.Models.Azure;

public class KeyVault : DataChunk
{
    public KeyVault(string? id, string name, string url)
    {
        Id = id;
        Name = name;
        Url = url;
    }

    [JsonProperty]
    public string? Id { get; private set;  }

    [JsonProperty]
    public string Name { get; private set; }
    
    [JsonProperty]
    public string Url { get; private set; }

    public override string DisplayName => Name;
}
