// See https://aka.ms/new-console-template for more information

using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

Console.WriteLine("Hello, World!");

var azureKeyVaultName = "testy-kv";
var secretClient = new SecretClient(new Uri($"https://{azureKeyVaultName}.vault.azure.net/"), new DefaultAzureCredential());
foreach (var page in secretClient.GetPropertiesOfSecrets().AsPages())
{
    foreach (var s in page.Values)
    {
        Console.WriteLine(s.Name);
    }
}
