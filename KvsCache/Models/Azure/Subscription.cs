namespace KvsCache.Models.Azure;

public class Subscription : DataChunk
{
    public Subscription(string? id, string name, string? tenantId)
    {
        Id = id;
        Name = name;
        TenantId = tenantId;
    }

    public string? Id { get; }
    public string Name { get; }
    public string? TenantId { get; }

    public override string DisplayName => Name;
}
