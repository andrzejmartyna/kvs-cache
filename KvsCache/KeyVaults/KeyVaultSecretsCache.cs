using KvsCache.Models;
using Newtonsoft.Json;

namespace KvsCache.KeyVaults;

public class KeyVaultSecretsCache
{
    public string SchemaVersion { get; init; } = "1.0";
    public DateTime ReadStartedAt { get; init; }
    public List<Subscription> Subscriptions { get; init; } = new();

    public int SubscriptionCount => Subscriptions.Count;
    public int KeyVaultCount => Subscriptions.Sum(s => s.KeyVaults.Count);
    public int SecretCount => Subscriptions.Sum(s => s.KeyVaults.Sum(kv => kv.Secrets.Count));

    public static KeyVaultSecretsCache ObtainValidCache(string filePath, TimeSpan maxAge, KeyVaultSecretsRepository keyVaultSecretsRepository)
    {
        var cache = ReadCacheFromFile(filePath);
        if (cache == null || !cache.IsValidAge(maxAge))
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

    public bool IsValidAge(TimeSpan maxAge) => DateTime.Now - ReadStartedAt <= maxAge;

    public static KeyVaultSecretsCache? ReadCacheFromFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            return JsonConvert.DeserializeObject<KeyVaultSecretsCache>(File.ReadAllText(filePath));
        }
        return null;
    }

    private void WriteCacheToFile(string filePath)
    {
        File.WriteAllText(filePath, JsonConvert.SerializeObject(this, Formatting.Indented));
    }
}
