using KvsCache.Browse;
using Newtonsoft.Json;

namespace KvsCache.KeyVaults;

public class KeyVaultSecretsCache
{
    public string SchemaVersion { get; init; } = "2.0";
    
    public DateTime CachedAt { get; init; }
    //TODO: change back to to List<Subscription> 
    public List<BrowserItem> Subscriptions { get; init; } = new();

    public string SubscriptionCount => Subscriptions.Count.ToString();
    public string KeyVaultCount => "?"; // Subscriptions.Sum(s => s.KeyVaults.Count);
    public string SecretCount => "?"; //Subscriptions.Sum(s => s.KeyVaults.Sum(kv => kv.Secrets.Count));

    public bool IsValidAge(TimeSpan maxAge) => DateTime.Now - CachedAt <= maxAge;

    public static KeyVaultSecretsCache ObtainValidCache(string filePath, TimeSpan maxAge, KeyVaultSecretsRepository keyVaultSecretsRepository)
    {
        var cache = ReadFromFile(filePath);
        if (cache == null || !cache.IsValidAge(maxAge))
        {
            cache = ReadFromAzure(filePath, keyVaultSecretsRepository);
            //TODO: rescan and reread all existing BrowserItem2 which are too old
        }
        return cache;
    }

    public static KeyVaultSecretsCache ReadFromAzure(string filePath, KeyVaultSecretsRepository keyVaultSecretsRepository)
    {
        var cache = new KeyVaultSecretsCache
        {
            Subscriptions = keyVaultSecretsRepository.GetSubscriptions().ToList(),
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
        File.WriteAllText(filePath, JsonConvert.SerializeObject(this, Formatting.Indented));
    }
}
