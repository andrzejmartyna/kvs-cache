using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.Security.KeyVault.Secrets;
using KvsCache.Models.Azure;
using KvsCache.Models.Errors;

namespace KvsCache.Harvest;

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
                new Subscription(s.Id, s.Data.DisplayName, s.Data.TenantId?.ToString())).ToList();
        });

    public OneOrError<List<KeyVault>> GetKeyVaults(string subscriptionName) =>
        ProtectFromRequestFailing<List<KeyVault>>(() =>
        {
            var client = new ArmClient(_credentials);
            var subscriptionResourceOrError = ProtectFromRequestFailing<SubscriptionResource>(() =>
            {
                foreach (var s in client.GetSubscriptions())
                {
                    if (0 == string.Compare(subscriptionName, s.Data.DisplayName, StringComparison.OrdinalIgnoreCase))
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

            var keyVaultResources = subscriptionResource.GetGenericResources("resourceType eq 'Microsoft.KeyVault/vaults'").ToList();
            return (from kv in keyVaultResources
                let azureKeyVaultName = kv.Data.Name
                let azureKeyVaultUrl = BuildKeyVaultUrl(azureKeyVaultName)
                select new KeyVault(kv.Id, azureKeyVaultName, azureKeyVaultUrl)).ToList();
        });

    public OneOrError<List<Secret>> GetSecrets(string azureKeyVaultUrl) =>
        ProtectFromRequestFailing<List<Secret>>(() =>
        {
            var secretClient = new SecretClient(new Uri(azureKeyVaultUrl), _credentials);
            return secretClient.GetPropertiesOfSecrets().Select(sp => new Secret(sp.Id.ToString(), sp.Name)).ToList();
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
