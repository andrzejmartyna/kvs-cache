using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.Security.KeyVault.Secrets;
using KvsCache.Models.Azure;
using KvsCache.Models.Errors;

namespace KvsCache.KeyVaults;

public class KeyVaultSecretsRepository
{
    private readonly TokenCredential _credentials = new ChainedTokenCredential(new AzureCliCredential(), new DefaultAzureCredential(true));

    public static string BuildResourceAzurePortalUrl(string tenantId, string resourceId) => $"https://portal.azure.com/#@{tenantId}/resource/{resourceId}";

    private static string BuildKeyVaultUrl(string keyVaultName) => $"https://{keyVaultName}.vault.azure.net/";
    
    public OneOrError<List<Subscription>> GetSubscriptions() =>
        ProtectFromRequestFailing<List<Subscription>>(() =>
        {
            var client = new ArmClient(_credentials);
            return client.GetSubscriptions().Select(s =>
                new Subscription(s.Id, s.Data.DisplayName, s.Data.ManagedByTenants.ToString())).ToList();
        });

    public OneOrError<List<KeyVault>> GetKeyVaults(Subscription subscription) =>
        ProtectFromRequestFailing<List<KeyVault>>(() =>
        {
            var client = new ArmClient(_credentials);
            var subscriptionResourceOrError = ProtectFromRequestFailing<SubscriptionResource>(() =>
            {
                foreach (var s in client.GetSubscriptions())
                {
                    if (0 == string.Compare(subscription.Name, s.Data.DisplayName, StringComparison.OrdinalIgnoreCase))
                    {
                        return s;
                    }
                }
                return new ErrorNotFound("Subscription");
            });

            if (subscriptionResourceOrError.TryPickT1(out var error, out var subscriptionResource))
            {
                return error;
            }
            
            var keyVaults = new List<KeyVault>();
            var keyVaultResources = subscriptionResource.GetGenericResources("resourceType eq 'Microsoft.KeyVault/vaults'").ToList();
            foreach (var kv in keyVaultResources)
            {
                var azureKeyVaultName = kv.Data.Name;
                var azureKeyVaultUrl = BuildKeyVaultUrl(azureKeyVaultName);
                keyVaults.Add(new KeyVault(kv.Id, azureKeyVaultName, azureKeyVaultUrl));
            }
            return keyVaults;
        });

    public OneOrError<List<Secret>> GetSecrets(string azureKeyVaultUrl) =>
        ProtectFromRequestFailing<List<Secret>>(() =>
        {
            var secrets = new List<Secret>();
            var secretClient = new SecretClient(new Uri(azureKeyVaultUrl), _credentials);
            foreach (var sp in secretClient.GetPropertiesOfSecrets())
            {
                secrets.Add(new Secret(sp.Id.ToString(), sp.Name));
            }
            return secrets;
        });

    public OneOrError<string> GetSecretValue(string keyVaultUrl, string secretName) =>
        ProtectFromRequestFailing<string>(() =>
        {
            var secretClient = new SecretClient(new Uri(keyVaultUrl), _credentials);
            return secretClient.GetSecret(secretName).Value.Value;
        });

    private static OneOrError<T> ProtectFromRequestFailing<T>(Func<OneOrError<T>> func)
    {
        var errorMessage = string.Empty;
        try
        {
            return func();
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
        return new ErrorInfo(errorMessage);
    }
}
