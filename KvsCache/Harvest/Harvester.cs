using KvsCache.Models.Azure;
using KvsCache.Models.Errors;

namespace KvsCache.Harvest;

public class Harvester
{
    //TODO: private readonly string _cacheFile = "kvs-cache.json";
    private readonly KeyVaultSecretsRepository _keyVaultSecretsRepository = new();

    public string SubscriptionCount => "?"; //TODO: Subscriptions.Count.ToString();
    public string KeyVaultCount => "?"; //TODO: Subscriptions.Sum(s => s.KeyVaults.Count);
    public string SecretCount => "?"; //TODO: Subscriptions.Sum(s => s.KeyVaults.Sum(kv => kv.Secrets.Count));

    public OneOrError<List<Subscription>> GetSubscriptions() => _keyVaultSecretsRepository.GetSubscriptions();
    public OneOrError<List<KeyVault>> GetKeyVaults(Subscription subscription) => _keyVaultSecretsRepository.GetKeyVaults(subscription);
    public OneOrError<List<Secret>> GetSecrets(string azureKeyVaultUrl) => _keyVaultSecretsRepository.GetSecrets(azureKeyVaultUrl);
    public OneOrError<string> GetSecretValue(string keyVaultUrl, string secretName) => _keyVaultSecretsRepository.GetSecretValue(keyVaultUrl, secretName);
}
