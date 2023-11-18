using Azure.Identity;
using Azure.ResourceManager;
using Azure.Security.KeyVault.Secrets;

namespace kcs_cache;

public static class KeyVaultSecretsRepository
{
    public static IEnumerable<KeyVaultSecretEntry> GetKeyVaultSecretEntries()
    {
        var credentials = new DefaultAzureCredential();
        var client = new ArmClient(credentials);

        foreach (var subscription in client.GetSubscriptions())
        {
            foreach (var obj in subscription.GetGenericResources("resourceType eq 'Microsoft.KeyVault/vaults'"))
            {
                var azureKeyVaultName = obj.Data.Name;
                var azureKeyVaultUrl = $"https://{azureKeyVaultName}.vault.azure.net/";
                var secretClient = new SecretClient(new Uri(azureKeyVaultUrl), credentials);
                foreach (var page in secretClient.GetPropertiesOfSecrets().AsPages())
                {
                    foreach (var sp in page.Values)
                    {
                        yield return new KeyVaultSecretEntry(subscription.Data.DisplayName, subscription.Id, azureKeyVaultName, azureKeyVaultUrl, sp.Name);
                    }
                }
            }
        }
    }
}
