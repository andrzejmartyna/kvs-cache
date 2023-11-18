// See https://aka.ms/new-console-template for more information

using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using kcs_cache;

var cacheFile = "kcs-cache.json";
var currentCache = KeyVaultSecretsCache.ObtainValidCache(cacheFile, new TimeSpan(1));

Console.WriteLine($"Key vaults secrets gathered at: {currentCache.ReadStartedAt}");
Console.WriteLine($"Found {currentCache.Entries.Count} secrets in {currentCache.Entries.Select(a => a.KeyVault).Distinct().Count()} Key Vault(s) in {currentCache.Entries.Select(a => a.Subscription).Distinct().Count()} subscription(s)");

while (true)
{
    Console.WriteLine("Choose options: (1) search cached secrets, (2) refresh cache, (9) exit");
    var option = Console.ReadKey().KeyChar;
    if (option == '9')
    {
        break;
    }
    
    switch (option)
    {
        case '1':
            Console.WriteLine("Provide one or more words separated with spaces.");
            Console.WriteLine("Secret, Key vault and subscription names will be searched.");
            Console.WriteLine("Only secrets which have all words found within will be returned.");
            var words = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(words))
            {
                var searchResult = currentCache.SearchForWords(words.Split(' ')).ToList();
                Console.WriteLine($"Found {searchResult.Count()} secrets");
                var counter = 0;
                if (searchResult.Any())
                {
                    foreach (var entry in searchResult)
                    {
                        Console.WriteLine($"{counter++}: {entry.Subscription}/{entry.KeyVault}/{entry.SecretName}");
                    }
                    Console.WriteLine("Enter number from the list to get secret value into clipboard");
                    var num = int.Parse(Console.ReadLine());
                    if (num >= 0 && num < counter)
                    {
                        var chosenEntry = searchResult[num];
                        var credentials = new DefaultAzureCredential();
                        var secretClient = new SecretClient(new Uri(chosenEntry.KeyVaultUrl), credentials);

                        var secretValue = secretClient.GetSecret(chosenEntry.SecretName).Value;
                        Clipboard.SetText(secretValue.Value);
                        
                        Console.WriteLine($"Subscription: {chosenEntry.Subscription}\r\nKeyVault: {chosenEntry.KeyVault}\r\nSecret: {chosenEntry.SecretName}");
                        Console.WriteLine("Value of above secret was copied to the clipboard.");
                    }
                }
            }
            break;
        case '2':
            currentCache = KeyVaultSecretsCache.Refresh("kcs-cache.json");
            break;
    }
}
