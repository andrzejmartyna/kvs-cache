namespace kcs_cache.Models;

public record Subscription(string Id, string Name, string? TenantId, List<KeyVault> KeyVaults);
