using System.Text.Json;

namespace kcs_cache;

public class KeyVaultSecretsCache
{
    public DateTime ReadStartedAt { get; init; }
    public List<KeyVaultSecretEntry> Entries { get; init; } = new List<KeyVaultSecretEntry>();
    
    public static KeyVaultSecretsCache Refresh(string filePath)
    {
        return ObtainValidCache(filePath, new TimeSpan());
    }
    
    public static KeyVaultSecretsCache ObtainValidCache(string filePath, TimeSpan maxAge)
    {
        var cache = ReadCacheFromFile(filePath);
        if (cache == null || DateTime.Today - cache.ReadStartedAt > maxAge)
        {
            cache = new KeyVaultSecretsCache
            {
                ReadStartedAt = DateTime.Now,
                Entries = KeyVaultSecretsRepository.GetKeyVaultSecretEntries().ToList()
            };
            
            cache.WriteCacheToFile(filePath);
        }
        return cache;
    }
    
    protected static KeyVaultSecretsCache? ReadCacheFromFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            return JsonSerializer.Deserialize<KeyVaultSecretsCache>(File.ReadAllText(filePath));
        }
        return null;
    }

    public void WriteCacheToFile(string filePath)
    {
        File.WriteAllText(filePath, JsonSerializer.Serialize(this));
    }

    public IEnumerable<KeyVaultSecretEntry> SearchForWords(string[] words)
    {
        var currentList = Entries;
        foreach (var word in words)
        {
            currentList = currentList.Where(a
                => a.Subscription.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                   || a.KeyVault.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                   || a.SecretName.Contains(word, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }
        return currentList;
    }
}
