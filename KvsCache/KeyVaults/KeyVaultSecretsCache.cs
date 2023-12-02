using KvsCache.Models.Azure;
using KvsCache.Models.Errors;
using Newtonsoft.Json;

namespace KvsCache.KeyVaults;

public class KeyVaultSecretsCache
{
    public string SchemaVersion { get; init; } = "2.0";
    
    public DateTime CachedAt { get; private init; }
    public List<Subscription> Subscriptions { get; private init; } = new();

    public string SubscriptionCount => Subscriptions.Count.ToString();
    public string KeyVaultCount => "?"; // Subscriptions.Sum(s => s.KeyVaults.Count);
    public string SecretCount => "?"; //Subscriptions.Sum(s => s.KeyVaults.Sum(kv => kv.Secrets.Count));

    public bool IsValidAge(TimeSpan maxAge) => DateTime.Now - CachedAt <= maxAge;

    public static OneOrError<KeyVaultSecretsCache> ObtainValidCache(string filePath, TimeSpan maxAge, KeyVaultSecretsRepository keyVaultSecretsRepository)
    {
        var cache = ReadFromFile(filePath);
        if (cache != null && cache.IsValidAge(maxAge))
        {
            return cache;
        }

        //TODO: rescan and reread all existing BrowserItem2 which are too old
        return ReadFromAzure(filePath, keyVaultSecretsRepository);
    }

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
