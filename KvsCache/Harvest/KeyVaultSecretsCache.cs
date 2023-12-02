using KvsCache.Models.Azure;
using KvsCache.Models.Errors;

namespace KvsCache.Harvest;

public class KeyVaultSecretsCache
{
    public string SchemaVersion { get; init; } = "2.0";
    
    public DateTime CachedAt { get; private init; }
    public List<Subscription> Subscriptions { get; private init; } = new();

    public static OneOrError<KeyVaultSecretsCache> ReadFromAzure(string filePath, KeyVaultSecretsRepository keyVaultSecretsRepository)
    {
        var subscriptionsOrError = keyVaultSecretsRepository.GetSubscriptions();
        if (subscriptionsOrError.TryPickT1(out var error, out var list))
        {
            return error;
        }
        var cache = new KeyVaultSecretsCache
        {
            Subscriptions = list,
            CachedAt = DateTime.Now
        };
        cache.WriteCacheToFile(filePath);
        return cache;
    }
    
    public static KeyVaultSecretsCache? ReadFromFile(string filePath)
        //TODO: provide proper serialization
        => null;//File.Exists(filePath) ? JsonConvert.DeserializeObject<KeyVaultSecretsCache>(File.ReadAllText(filePath)) : null;
    
    private void WriteCacheToFile(string filePath)
    {
        //TODO: write it again
        //File.WriteAllText(filePath, JsonConvert.SerializeObject(this, Formatting.Indented));
    }
}
