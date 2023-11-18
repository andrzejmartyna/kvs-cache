using kcs_cache.Browse;
using kcs_cache.ConsoleDraw;
using kcs_cache.KeyVaults;
using kcs_cache.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace kcs_cache;

public class Controller
{
    private KeyVaultSecretsCache _currentCache = new();
    private readonly ConsoleUi _console;
    private readonly BrowseGeometry _geometry;
    private readonly KeyVaultSecretsRepository _keyVaultSecretsRepository = new();

    public Controller(Rectangle operationRectangle)
    {
        _geometry = new BrowseGeometry(operationRectangle);
        _console = new ConsoleUi(_geometry);
    }

    public void Execute()
    {
        Console.CursorVisible = false;
        
        var cacheFile = "kvs-cache.json";
        _currentCache = KeyVaultSecretsCache.ObtainValidCache(cacheFile, new TimeSpan(1, 0, 0, 0), _keyVaultSecretsRepository);
        
        InitialDraw();
        BrowseSubscriptions();
        OnExit();
        
        Console.CursorVisible = true;
    }

    private void BrowseSubscriptions()
    {
        new Browser(_console, _currentCache.Subscriptions.Select(a => (a.Name, (object)a)), null, BrowseKeyVaults, "Subscriptions").Browse();
    }

    private void BrowseKeyVaults(BrowserItem selected, bool altPressed)
    {
        var selection = _geometry.SelectionRectangle;
        _console.WriteAt(selection.Left, selection.Top, selected.DisplayName);
        new Browser(_console, _currentCache.Subscriptions.SelectMany(a => a.KeyVaults.Select(a => (a.Name, (object)a))), selected, BrowseSecrets, "KeyVaults").Browse();
        _console.WriteAt(selection.Left, selection.Top, new string(' ', selection.Width));
    }

    private void BrowseSecrets(BrowserItem selected, bool altPressed)
    {
        var selection = _geometry.SelectionRectangle;
        _console.WriteAt(selection.Left, selection.Top + 1, selected.DisplayName);
        new Browser(_console, _currentCache.Subscriptions.SelectMany(a => a.KeyVaults.SelectMany(a => a.Secrets.Select(a => (a.Name, (object)a)))), selected, ReadSecretValue, "Secrets").Browse();
        _console.WriteAt(selection.Left, selection.Top + 1, new string(' ', selection.Width));
    }

    private void ReadSecretValue(BrowserItem selected, bool altPressed)
    {
        var subscription = (Subscription?)selected.Parent?.Parent?.Items[0];
        var keyVault = (KeyVault?)selected.Parent?.Items[0];
        var secret = (Secret)selected.Items[0];
        if (keyVault == null)
        {
            _console.Message($"Internal error - no KeyVault found for the {secret.Name} secret", _console.RedMessage);
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
            _console.Message("The clipboard was filled with full information about the secret.", _console.GreenMessage);
        }
        else
        {
            Clipboard.SetText(secretValue);
            _console.Message($"Value of the secret was copied to the clipboard.", _console.GreenMessage);
        }
    }

    public void DrawTestBoard()
    {
        _console.DrawDoubleRectangle(_geometry.Full);
        _console.DrawHorizontalLine(_geometry.DivideLine, true);
        _console.FillRectangle(_geometry.SummaryRectangle, 's');
        _console.FillRectangle(_geometry.SelectionRectangle, 'x');
        _console.FillRectangle(_geometry.RefreshedRectangle, 'r');
        _console.FillRectangle(_geometry.BrowsingRectangle, '.');
        _console.FillRectangle(_geometry.TipsRectangle, 't');
        _console.FillRectangle(_geometry.SelectionHeaderLine.Rectangle, 'u');
        Console.SetCursorPosition(0,  _geometry.Full.Bottom);
        Console.WriteLine();
    }

    private void InitialDraw()
    {
        _console.DrawDoubleRectangle(_geometry.Full);
        _console.DrawHorizontalLine(_geometry.DivideLine, true);
        
        var info = _geometry.SummaryRectangle;
        _console.WriteAt(info.Left, info.Top + 0, $"Subscriptions: {_currentCache.SubscriptionCount}");
        _console.WriteAt(info.Left, info.Top + 1, $"   Key vaults: {_currentCache.KeyVaultCount}");
        _console.WriteAt(info.Left, info.Top + 2, $"      Secrets: {_currentCache.SecretCount}");

        var refreshed = _geometry.RefreshedRectangle;
        var refreshedAt = $"Refreshed at {_currentCache.ReadStartedAt}";
        _console.WriteAt(refreshed.Right - refreshedAt.Length, refreshed.Top, refreshedAt);

        var tips = _geometry.TipsRectangle;
        _console.WriteAt(tips.Left, tips.Top, "Arrow keys / Enter / Alt-Enter / Esc");
        const string commands = "Ctrl-R Refresh / Ctrl-C Exit";
        _console.WriteAt(tips.Right - commands.Length, tips.Top, commands);
    }

    public void OnExit()
    {
        _console.FillRectangle(_geometry.Full, ' ');
        Console.SetCursorPosition(0,  _geometry.Full.Top + 1);
        Console.WriteLine("Thank you for using kvs-cache!");
    }
}
