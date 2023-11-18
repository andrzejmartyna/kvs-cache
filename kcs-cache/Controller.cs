using kcs_cache.Browse;
using kcs_cache.ConsoleDraw;
using kcs_cache.KeyVaults;
using kcs_cache.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace kcs_cache;

public class Controller
{
    private KeyVaultSecretsCache _currentCache = new KeyVaultSecretsCache();
    private ConsoleUi _console = new ConsoleUi(80, 20);

    private KeyVaultSecretsRepository _keyVaultSecretsRepository = new KeyVaultSecretsRepository();
    
    public void Start()
    {
        Console.CursorVisible = false;
        
        var cacheFile = "kcs-cache.json";
        _currentCache = KeyVaultSecretsCache.ObtainValidCache(cacheFile, new TimeSpan(1, 0, 0, 0), _keyVaultSecretsRepository);
        
        InitialDraw();
        BrowseSubscriptions();
        
        Console.CursorVisible = true;
    }

    private void BrowseSubscriptions()
    {
        new Browser(_console, _currentCache.Subscriptions.Select(a => (a.Name, (object)a)), null, BrowseKeyVaults, "Subscriptions").Browse();
    }

    private void BrowseKeyVaults(BrowserItem selected, bool altPressed)
    {
        new Browser(_console, _currentCache.Subscriptions.SelectMany(a => a.KeyVaults.Select(a => (a.Name, (object)a))), selected, BrowseSecrets, "KeyVaults").Browse();
    }

    private void BrowseSecrets(BrowserItem selected, bool altPressed)
    {
        new Browser(_console, _currentCache.Subscriptions.SelectMany(a => a.KeyVaults.SelectMany(a => a.Secrets.Select(a => (a.Name, (object)a)))), selected, ReadSecretValue, "Secrets").Browse();
    }

    private void ReadSecretValue(BrowserItem selected, bool altPressed)
    {
        var subscription = (Subscription?)selected.Parent?.Parent?.Items[0];
        var keyVault = (KeyVault?)selected.Parent?.Items[0];
        var secret = (Secret)selected.Items[0];
        if (keyVault == null)
        {
            _console.Message($"Internal error - no KeyVault found for the {secret.Name} secret");
            return;
        }

        var secretValue = _keyVaultSecretsRepository.GetSecretValue(keyVault.Url, secret.Name);

        if (altPressed)
        {
            var jsonObject = new JObject
            {
                { "Subscription", new JObject
                    {
                        {"Id", subscription?.Id},
                        {"Name", subscription?.Name},
                        //TODO: find out why TenantId is null
                        //{"TenantId", subscription.TenantId},
                        //{"AzurePortalUrl", KeyVaultSecretsRepository.BuildResourceAzurePortalUrl(subscription.TenantId, subscription.Id)}
                    }
                },
                { "KeyVault", new JObject
                    {
                        {"Name", keyVault.Name},
                        {"Url", keyVault.Url},
                        //TODO: find out why TenantId is null
                        //{"AzurePortalUrl", KeyVaultSecretsRepository.BuildResourceAzurePortalUrl(subscription.TenantId, keyVault.Id)}
                    }
                },
                { "Secret", new JObject
                    {
                        {"Name", secret.Name},
                        {"Value", secretValue},
                        //TODO: find out why TenantId is null
                        //{"AzurePortalUrl", KeyVaultSecretsRepository.BuildResourceAzurePortalUrl(subscription.TenantId, secret.Id)}
                    }
                }
            };
            
            Clipboard.SetText(JsonConvert.SerializeObject(jsonObject, Formatting.Indented));
            _console.Message("The clipboard was filled with full information about the secret.");
        }
        else
        {
            Clipboard.SetText(secretValue);
            _console.Message($"Value of the secret was copied to the clipboard.");
        }
    }

    private void InitialDraw()
    {
        _console.DrawDoubleRectangle(0, 0, _console.Width - 1, _console.Height - 1);
        _console.DrawHorizontalLine(1, 4, _console.Width - 2);
        _console.WriteAt(2, 1, $"Subscriptions: {_currentCache.SubscriptionCount}");
        _console.WriteAt(2, 2, $"   Key vaults: {_currentCache.KeyVaultCount}");
        _console.WriteAt(2, 3, $"      Secrets: {_currentCache.SecretCount}");
        var refreshedAt = $"Refreshed at {_currentCache.ReadStartedAt}";
        _console.WriteAt(_console.Width - refreshedAt.Length - 2, 3, refreshedAt);
        _console.WriteAt(2, _console.Height - 1, "Filter / Up,Down,Home,End / Enter / Alt-Enter / Esc");
        const string commands = "Ctrl-R Refresh / Ctrl-C Exit";
        _console.WriteAt(_console.Width - commands.Length - 2, _console.Height - 2, commands);

        const string filter = "Filter: ";
        var filterPosition = ( filter.Length + 2, 2 );
        _console.WriteAt(2, 5, filter);
        _console.SetCursorPosition(filterPosition.Item1, filterPosition.Item2);
    }
}