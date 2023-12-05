using Newtonsoft.Json;

namespace KvsCache.Models.Azure;

public class Subscription : DataChunk
{
    public Subscription(string? id, string name, string? tenantId)
    {
        Id = id;
        Name = name;
        TenantId = tenantId;
    }

    [JsonProperty]
    public string? Id { get; private set; }

    [JsonProperty]
    public string Name { get; private set; }

    [JsonProperty]
    public string? TenantId { get; private set; }

    public override string DisplayName => Name;
}
