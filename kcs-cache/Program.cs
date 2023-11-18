// See https://aka.ms/new-console-template for more information

using Azure.Identity;
using Azure.ResourceManager;
using Azure.Security.KeyVault.Secrets;

var creds = new DefaultAzureCredential();
var client = new ArmClient(creds);

foreach (var subscription in client.GetSubscriptions())
{
    Console.WriteLine($"Found subscription: {subscription.Data.DisplayName}");
    foreach (var obj in subscription.GetGenericResources("resourceType eq 'Microsoft.KeyVault/vaults'"))
    {
        var azureKeyVaultName = obj.Data.Name;
        Console.WriteLine($"Found KV: {azureKeyVaultName}");
        var secretClient = new SecretClient(new Uri($"https://{azureKeyVaultName}.vault.azure.net/"), creds);
        foreach (var page in secretClient.GetPropertiesOfSecrets().AsPages())
        {
            foreach (var s in page.Values)
            {
                Console.WriteLine($"Found secret: {s.Name} with value {secretClient.GetSecret(s.Name).Value.Value}");
            }
        }
    }
}
