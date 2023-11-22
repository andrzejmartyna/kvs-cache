namespace KvsCache.Models;

public record SubscriptionInfo(string? Id, string? Name
//             //TODO: find out why TenantId is null
//             //{"TenantId", subscription.TenantId},
//             //{"AzurePortalUrl", KeyVaultSecretsRepository.BuildResourceAzurePortalUrl(subscription.TenantId, subscription.Id)}
);
public record KeyVaultInfo(string Name, string Url
//             //TODO: find out why TenantId is null
//             //{"AzurePortalUrl", KeyVaultSecretsRepository.BuildResourceAzurePortalUrl(subscription.TenantId, keyVault.Id)}
);

public record SecretInfo(string Name
//             //TODO: find out why TenantId is null
//             //{"AzurePortalUrl", KeyVaultSecretsRepository.BuildResourceAzurePortalUrl(subscription.TenantId, secret.Id)}
);

public record SecretFullInfo(SubscriptionInfo Subscription, KeyVaultInfo KeyVault, SecretInfo Secret);
