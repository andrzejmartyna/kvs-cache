using kcs_cache.Models;
using Newtonsoft.Json;

namespace kcs_cache.KeyVaults;

public class KeyVaultSecretsCache
{
    public string SchemaVersion { get; init; } = "1.0";
    public DateTime ReadStartedAt { get; init; }
    public List<Subscription> Subscriptions { get; init; } = new List<Subscription>();

    public int SubscriptionCount => Subscriptions.Count;
    public int KeyVaultCount => Subscriptions.Sum(s => s.KeyVaults.Count);
    public int SecretCount => Subscriptions.Sum(s => s.KeyVaults.Sum(kv => kv.Secrets.Count));

    public static KeyVaultSecretsCache Refresh(string filePath, KeyVaultSecretsRepository keyVaultSecretsRepository)
    {
        return ObtainValidCache(filePath, new TimeSpan(), keyVaultSecretsRepository);
    }
    
    public static KeyVaultSecretsCache ObtainValidCache(string filePath, TimeSpan maxAge, KeyVaultSecretsRepository keyVaultSecretsRepository)
    {
        var cache = ReadCacheFromFile(filePath);
        if (cache == null || DateTime.Today - cache.ReadStartedAt > maxAge)
        {
            cache = new KeyVaultSecretsCache
            {
                ReadStartedAt = DateTime.Now,
                Subscriptions = keyVaultSecretsRepository.GetKeyVaultSecretEntries().ToList()
            };
            
            cache.WriteCacheToFile(filePath);
        }
        return cache;
    }
    
    protected static KeyVaultSecretsCache? ReadCacheFromFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            return JsonConvert.DeserializeObject<KeyVaultSecretsCache>(File.ReadAllText(filePath));
        }
        return null;
    }

    public void WriteCacheToFile(string filePath)
    {
        File.WriteAllText(filePath, JsonConvert.SerializeObject(this, Formatting.Indented));
    }
}
