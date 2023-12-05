using Newtonsoft.Json;

namespace KvsCache.Models.Azure;

public class Secret : DataItem
{
    public Secret(string id, string name)
    {
        Id = id;
        Name = name;
    }

    [JsonProperty]
    public string Id { get; private set;  }

    [JsonProperty]
    public string Name { get; private set; }

    public override string DisplayName => Name;
}

