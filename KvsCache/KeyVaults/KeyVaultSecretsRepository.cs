using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.Security.KeyVault.Secrets;
using KvsCache.Browse;
using KvsCache.Models.Azure;

namespace KvsCache.KeyVaults;

public class KeyVaultSecretsRepository
{
    private TokenCredential _credentials = new ChainedTokenCredential(new AzureCliCredential(), new DefaultAzureCredential(true));

    public static string BuildResourceAzurePortalUrl(string tenantId, string resourceId) => $"https://portal.azure.com/#@{tenantId}/resource/{resourceId}";

    public static string BuildKeyVaultUrl(string keyVaultName) => $"https://{keyVaultName}.vault.azure.net/";


    public IEnumerable<BrowserItem> GetKeyVaults(BrowserItem subscription)
    {
        var client = new ArmClient(_credentials);

        SubscriptionResource? subscriptionResource = null;

        var errorMessage = ProtectFromRequestFailing(() =>
        {
            foreach (var s in client.GetSubscriptions())
            {
                if (0 == string.Compare(subscription.DisplayName, s.Data.DisplayName, StringComparison.OrdinalIgnoreCase))
                {
                    subscriptionResource = s;
                    break;
                }
            }
        });

        if (subscriptionResource == null)
        {
            yield return new BrowserItem(BrowserItemType.Error, null, null, null, errorMessage, DateTime.Now);
        }
        else
        {
            foreach (var kv in GetKeyVaults(subscriptionResource, subscription))
            {
                yield return kv;
            }
        }
    }
    
    private IEnumerable<BrowserItem> GetKeyVaults(SubscriptionResource subscription, BrowserItem parent)
    {
        List<GenericResource>? keyVaults = null;
        var errorMessage = ProtectFromRequestFailing(() =>
        {
            keyVaults = subscription.GetGenericResources("resourceType eq 'Microsoft.KeyVault/vaults'").ToList();
        });

        if (keyVaults == null)
        {
            yield return new BrowserItem(BrowserItemType.Error, null, null, null, errorMessage, DateTime.Now);
        }
        else
        {
            foreach (var obj in keyVaults)
            {
                var azureKeyVaultName = obj.Data.Name;
                var azureKeyVaultUrl = BuildKeyVaultUrl(azureKeyVaultName);
                yield return new BrowserItem(BrowserItemType.Fetched, new KeyVault(obj.Id, azureKeyVaultName, azureKeyVaultUrl, new List<Secret>()), null, parent, string.Empty, DateTime.Now);
            }
        }
    }

    public IEnumerable<BrowserItem> GetSecrets(BrowserItem parent)
    {
        KeyVault? kv = parent.Self;
        if (kv == null)
        {
            yield return new BrowserItem(BrowserItemType.Error, null, null, null, "Internal error - null KeyVault passed to GetSecrets", DateTime.Now);
        }
        else
        {
            foreach (var secret in GetSecrets(kv.Url, parent))
            {
                yield return secret;
            }
        }
    }
    
    private IEnumerable<BrowserItem> GetSecrets(string azureKeyVaultUrl, BrowserItem parent)
    {
        List<SecretProperties>? secrets = null;
        var errorMessage = ProtectFromRequestFailing(() =>
        {
            var secretClient = new SecretClient(new Uri(azureKeyVaultUrl), _credentials);
            secrets = secretClient.GetPropertiesOfSecrets().ToList();
        });

        if (secrets == null)
        {
            yield return new BrowserItem(BrowserItemType.Error, null, null, null, errorMessage, DateTime.Now);
        }
        else
        {
            foreach (var sp in secrets)
            {
                yield return new BrowserItem(BrowserItemType.Fetched, new Secret(sp.Id.ToString(), sp.Name), null, parent, string.Empty, DateTime.Now);
            }
        }
    }

    public string GetSecretValue(string keyVaultUrl, string secretName)
    {
        var result = string.Empty;
        var errorMessage = ProtectFromRequestFailing(() =>
        {
            var secretClient = new SecretClient(new Uri(keyVaultUrl), _credentials);
            result = secretClient.GetSecret(secretName).Value.Value;
        });
        return !string.IsNullOrWhiteSpace(result) ? result : errorMessage;
    }

    public IEnumerable<BrowserItem> GetSubscriptions()
    {
        var client = new ArmClient(_credentials);

        List<SubscriptionResource>? subscriptions = null;

        var errorMessage = ProtectFromRequestFailing(() =>
        {
            subscriptions = client.GetSubscriptions().ToList();
        });

        if (subscriptions == null)
        {
            yield return new BrowserItem(BrowserItemType.Error, null, null, null, errorMessage, DateTime.Now);
        }
        else
        {
            foreach (var s in subscriptions)
            {
                yield return new BrowserItem(BrowserItemType.Fetched, new Subscription(s.Id, s.Data.DisplayName, s.Data.ManagedByTenants.ToString(), new List<KeyVault>()), null, null, String.Empty, DateTime.Now);
            }
        }
    }
    
    private static string ProtectFromRequestFailing(Action action)
    {
        var errorMessage = string.Empty;
        try
        {
            action();
        }
        catch (AuthenticationFailedException e)
        {
            errorMessage = e.Message;
        }
        catch (RequestFailedException e)
        {
            errorMessage = e.Message;
        }
        catch (AggregateException aggregateException)
        {
            var first = true;
            foreach (var ex in aggregateException.InnerExceptions)
            {
                if (ex is not RequestFailedException)
                {
                    throw;
                }
                errorMessage += $"{(first ? "" : "\r\n")}{ex.Message}";
                first = false;
            }
        }
        return errorMessage;
    }
}
