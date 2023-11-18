using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.Security.KeyVault.Secrets;
using kcs_cache.Models;

namespace kcs_cache.KeyVaults;

public class KeyVaultSecretsRepository
{
    private DefaultAzureCredential _credentials = new();

    public IEnumerable<Subscription> GetKeyVaultSecretEntries()
    {
        var client = new ArmClient(_credentials);
        foreach (var subscription in client.GetSubscriptions())
        {
            yield return new Subscription(subscription.Id?.Name ?? string.Empty, subscription.Data.DisplayName, subscription.Data.TenantId?.ToString(), GetKeyVaults(subscription).ToList());
        }
    }

    public static string BuildResourceAzurePortalUrl(string tenantId, string resourceId) => $"https://portal.azure.com/#@{tenantId}/resource/{resourceId}";
    
    public static string BuildKeyVaultUrl(string keyVaultName) => $"https://{keyVaultName}.vault.azure.net/";
    
    private IEnumerable<KeyVault> GetKeyVaults(SubscriptionResource subscription)
    {
        foreach (var obj in subscription.GetGenericResources("resourceType eq 'Microsoft.KeyVault/vaults'"))
        {
            var azureKeyVaultName = obj.Data.Name;
            var azureKeyVaultUrl = BuildKeyVaultUrl(azureKeyVaultName);
            yield return new KeyVault(obj.Id, azureKeyVaultName, azureKeyVaultUrl, GetSecrets(azureKeyVaultUrl).ToList());
        }
    }

    private IEnumerable<Secret> GetSecrets(string azureKeyVaultUrl)
    {
        var secretClient = new SecretClient(new Uri(azureKeyVaultUrl), _credentials);
        foreach (var page in secretClient.GetPropertiesOfSecrets().AsPages())
        {
            foreach (var sp in page.Values)
            {
                yield return new Secret(sp.Id.ToString(), sp.Name);
            }
        }
    }

    public string GetSecretValue(string keyVaultUrl, string secretName)
    {
        var secretClient = new SecretClient(new Uri(keyVaultUrl), _credentials);
        return secretClient.GetSecret(secretName).Value.Value;
    }
}
