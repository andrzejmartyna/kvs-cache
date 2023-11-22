using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.Security.KeyVault.Secrets;
using KvsCache.Models;

namespace KvsCache.KeyVaults;

public class KeyVaultSecretsRepository
{
    private TokenCredential _credentials = new ChainedTokenCredential(new AzureCliCredential(), new DefaultAzureCredential(true));

    public IEnumerable<Subscription> GetKeyVaultSecretEntries()
    {
        var client = new ArmClient(_credentials);
        List<SubscriptionResource>? subscriptions = null;
        try
        {
            subscriptions = client.GetSubscriptions().ToList();
        }
        catch (RequestFailedException)
        {
            //TODO: what a shame to eat exception
        }
        catch (AggregateException aggregateException)
        {
            foreach (var ex in aggregateException.InnerExceptions)
            {
                if (ex is not RequestFailedException)
                {
                    throw;
                }
            }
        }

        if (subscriptions == null)
        {
            yield return new Subscription(string.Empty, "Error reading Subscriptions", string.Empty, new List<KeyVault>());
        }
        else
        {
            foreach (var subscription in subscriptions)
            {
                yield return new Subscription(subscription.Id?.Name ?? string.Empty, subscription.Data.DisplayName, subscription.Data.TenantId?.ToString(), GetKeyVaults(subscription).ToList());
            }
        }
    }

    public static string BuildResourceAzurePortalUrl(string tenantId, string resourceId) => $"https://portal.azure.com/#@{tenantId}/resource/{resourceId}";

    public static string BuildKeyVaultUrl(string keyVaultName) => $"https://{keyVaultName}.vault.azure.net/";

    private IEnumerable<KeyVault> GetKeyVaults(SubscriptionResource subscription)
    {
        List<GenericResource>? keyVaults = null;
        try
        {
            keyVaults = subscription.GetGenericResources("resourceType eq 'Microsoft.KeyVault/vaults'").ToList();
        }
        catch (RequestFailedException)
        {
            //TODO: what a shame to eat exception
        }
        catch (AggregateException aggregateException)
        {
            foreach (var ex in aggregateException.InnerExceptions)
            {
                if (ex is not RequestFailedException)
                {
                    throw;
                }
            }
        }

        if (keyVaults == null)
        {
            yield return new KeyVault(string.Empty, "Error reading KeyVaults", string.Empty, new List<Secret>());
        }
        else
        {
            foreach (var obj in keyVaults)
            {
                var azureKeyVaultName = obj.Data.Name;
                var azureKeyVaultUrl = BuildKeyVaultUrl(azureKeyVaultName);
                yield return new KeyVault(obj.Id, azureKeyVaultName, azureKeyVaultUrl, GetSecrets(azureKeyVaultUrl).ToList());
            }
        }
    }

    private IEnumerable<Secret> GetSecrets(string azureKeyVaultUrl)
    {
        List<SecretProperties>? secrets = null;
        try
        {
            var secretClient = new SecretClient(new Uri(azureKeyVaultUrl), _credentials);
            secrets = secretClient.GetPropertiesOfSecrets().ToList();
        }
        catch (RequestFailedException)
        {
            //TODO: what a shame to eat exception
        }
        catch (AggregateException aggregateException)
        {
            foreach (var ex in aggregateException.InnerExceptions)
            {
                if (ex is not RequestFailedException)
                {
                    throw;
                }
            }
        }

        if (secrets == null)
        {
            yield return new Secret(string.Empty, "Error reading Secrets");
        }
        else
        {
            foreach (var sp in secrets)
            {
                yield return new Secret(sp.Id.ToString(), sp.Name);
            }
        }
    }

    public string GetSecretValue(string keyVaultUrl, string secretName)
    {
        var secretClient = new SecretClient(new Uri(keyVaultUrl), _credentials);
        try
        {
            return secretClient.GetSecret(secretName).Value.Value;
        }
        catch (AuthenticationFailedException)
        {
            //TODO: what a shame to eat exception
            return "Error reading secret";
        }
        catch (RequestFailedException)
        {
            //TODO: what a shame to eat exception
            return "Error reading secret";
        }
        catch (AggregateException aggregateException)
        {
            foreach (var ex in aggregateException.InnerExceptions)
            {
                if (ex is not RequestFailedException)
                {
                    throw;
                }
            }
            return "Error reading secret";
        }
    }
}
