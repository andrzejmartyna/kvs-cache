using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace kcs_cache;

public class Controller
{
    private KeyVaultSecretsCache _currentCache;
    private ConsoleUi _console;
    
    public void Start()
    {
        Init();
        RunOldWay();
    }

    private void RunOldWay()
    {
        _console.MoveTo(1, _console.Height);

        Console.WriteLine($"Key vaults secrets gathered at: {_currentCache.ReadStartedAt}");
        Console.WriteLine($"Found {_currentCache.Entries.Count} secrets in {_currentCache.Entries.Select(a => a.KeyVault).Distinct().Count()} Key Vault(s) in {_currentCache.Entries.Select(a => a.Subscription).Distinct().Count()} subscription(s)");

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
                        var searchResult = _currentCache.SearchForWords(words.Split(' ')).ToList();
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
                    _currentCache = KeyVaultSecretsCache.Refresh("kcs-cache.json");
                    break;
            }
        }    
    }

    private void Init()
    {
        var cacheFile = "kcs-cache.json";
        _currentCache = KeyVaultSecretsCache.ObtainValidCache(cacheFile, new TimeSpan(1));

        _console = new ConsoleUi(80, 20);

        _console.DrawDoubleRectangle(0, 0, _console.Width - 1, _console.Height - 1);
        _console.DrawHorizontalLine(1, 4, _console.Width - 2);
        _console.WriteAt(2, 1, $"Subscriptions: {_currentCache.Entries.Select(a => a.Subscription).Distinct().Count()}");
        _console.WriteAt(2, 2, $"   Key vaults: {_currentCache.Entries.Select(a => a.KeyVault).Distinct().Count()}");
        _console.WriteAt(2, 3, $"      Secrets: {_currentCache.Entries.Count}");
        var refreshedAt = $"Refreshed at {_currentCache.ReadStartedAt}";
        _console.WriteAt(_console.Width - refreshedAt.Length - 2, 3, refreshedAt);
        _console.WriteAt(2, _console.Height - 2, "Filter / Up,Down,Home,End / Enter / Esc");
        const string commands = "Ctrl-R Refresh / Ctrl-C Exit";
        _console.WriteAt(_console.Width - commands.Length - 2, _console.Height - 2, commands);

        const string filter = "Filter: ";
        var filterPosition = ( filter.Length + 2, 2 );
        _console.WriteAt(2, 5, filter);
        _console.MoveTo(filterPosition.Item1, filterPosition.Item2);
    }
}