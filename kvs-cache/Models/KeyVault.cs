namespace kcs_cache.Models;

public record KeyVault(string? Id, string Name, string Url, List<Secret> Secrets);
