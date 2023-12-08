using Newtonsoft.Json;

namespace KvsCache.Models.Azure;

public abstract class DataItem
{
    [JsonIgnore]
    public abstract string DisplayName { get; }
}
