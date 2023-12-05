using Newtonsoft.Json;

namespace KvsCache.Models.Azure;

public class Subscriptions : DataChunk
{
    [JsonIgnore]
    public override string DisplayName => nameof(Subscriptions);
}
