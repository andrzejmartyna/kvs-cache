using System.Reflection;
using kcs_cache.Browse;
using kcs_cache.ConsoleDraw;
using kcs_cache.KeyVaults;
using kcs_cache.Models;
using Newtonsoft.Json;

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

    private void ReadingSecrets()
    {
        var cacheFile = "kvs-cache.json";
        _currentCache = KeyVaultSecretsCache.ObtainValidCache(cacheFile, new TimeSpan(1, 0, 0, 0), _keyVaultSecretsRepository);
    }
    
    public void Execute()
    {
        Console.CursorVisible = false;
        
        InitialDraw();
        
        Progress.Run(ReadingSecrets, _console, _geometry.RefreshedRectangle, "Collecting new information about secrets");

        DrawStatistics();
        BrowseSubscriptions();

        OnExit();
    }

    private void BrowseSubscriptions()
    {
        new Browser(_console, _currentCache.Subscriptions.Select(a => (a.Name, (object)a)), null, BrowseKeyVaults, null, false, "Subscriptions").Browse();
    }

    private void BrowseKeyVaults(BrowserItem selected)
    {
        var selection = _geometry.SelectionRectangle;
        _console.WriteAt(selection.Left, selection.Top, selected.DisplayName);
        new Browser(_console, _currentCache.Subscriptions.SelectMany(a => a.KeyVaults.Select(a => (a.Name, (object)a))), selected, BrowseSecrets, null, true, "KeyVaults").Browse();
        _console.WriteAt(selection.Left, selection.Top, new string(' ', selection.Width));
    }

    private void BrowseSecrets(BrowserItem selected)
    {
        var selection = _geometry.SelectionRectangle;
        _console.WriteAt(selection.Left, selection.Top + 1, selected.DisplayName);
        new Browser(_console, _currentCache.Subscriptions.SelectMany(a => a.KeyVaults.SelectMany(a => a.Secrets.Select(a => (a.Name, (object)a)))), selected, ReadSecretValue, InfoSecretValue, true, "Secrets").Browse();
        _console.WriteAt(selection.Left, selection.Top + 1, new string(' ', selection.Width));
    }

    private void InfoSecretValue(BrowserItem selected)
    {
        InfoOrReadSecretValue(selected, true);
    }

    private void ReadSecretValue(BrowserItem selected)
    {
        InfoOrReadSecretValue(selected, false);
    }
    
    private void InfoOrReadSecretValue(BrowserItem selected, bool info)
    {
        var subscription = (Subscription?)selected.Parent?.Parent?.Items[0];
        var keyVault = (KeyVault?)selected.Parent?.Items[0];
        var secret = (Secret)selected.Items[0];
        var secretValue = string.Empty;
        if (keyVault == null)
        {
            _console.Message($"Internal error - no KeyVault found for the {secret.Name} secret", _console.RedMessage);
            return;
        }

        var taskReading = new Task(() =>
        {
            secretValue = _keyVaultSecretsRepository.GetSecretValue(keyVault.Url, secret.Name);
        });
        
        Progress.Run(() =>
        {
            secretValue = _keyVaultSecretsRepository.GetSecretValue(keyVault.Url, secret.Name);
        }, _console, _geometry.ReadingProgressRectangle, "Reading");
        
        if (info)
        {
            var secretInfo = new SecretFullInfo(
                new SubscriptionInfo(subscription?.Id, subscription?.Name),
                new KeyVaultInfo(keyVault.Name, keyVault.Url),
                new SecretInfo(secret.Name, secretValue));
            Clipboard.SetText(JsonConvert.SerializeObject(secretInfo, Formatting.Indented));
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
        _console.FillRectangle(_geometry.ReadingProgressRectangle, '%');
        _console.FillRectangle(_geometry.BrowsingRectangle, '.');
        _console.FillRectangle(_geometry.TipsRectangle, 't');
        _console.FillRectangle(_geometry.SelectionHeaderLine.Rectangle, 'u');
        _console.FillRectangle(_geometry.VersionHeaderLine.Rectangle, 'v');
        Console.SetCursorPosition(0,  _geometry.Full.Bottom);
        Console.WriteLine();
    }

    public void TestKeys()
    {
        ConsoleKeyInfo key;
        do
        {
            key = Console.ReadKey();
            Console.WriteLine(JsonConvert.SerializeObject(key, Formatting.Indented));
        }
        while (key.Key != ConsoleKey.Escape);
    }

    private void InitialDraw()
    {
        _console.DrawDoubleRectangle(_geometry.Full);
        _console.DrawHorizontalLine(_geometry.DivideLine, true);

        var tips = _geometry.TipsRectangle;
        const string commands = "Ctrl-C Exit";
        _console.WriteAt(tips.Right - commands.Length + 1, tips.Top, commands);

        var versionInfo = _geometry.VersionHeaderLine.Rectangle;
        string version = $"kvs-cache v{Assembly.GetExecutingAssembly().GetName().Version?.ToString(2) ?? "..."}";
        _console.WriteAt(versionInfo.Right - version.Length + 1, versionInfo.Top, version);
    }

    private void DrawStatistics()
    {
        var info = _geometry.SummaryRectangle;
        _console.WriteAt(info.Left, info.Top + 0, $"Subscriptions: {_currentCache.SubscriptionCount}");
        _console.WriteAt(info.Left, info.Top + 1, $"   Key vaults: {_currentCache.KeyVaultCount}");
        _console.WriteAt(info.Left, info.Top + 2, $"      Secrets: {_currentCache.SecretCount}");

        var refreshed = _geometry.RefreshedRectangle;
        var refreshedAt = $"Refreshed at {_currentCache.ReadStartedAt}";
        _console.WriteAt(refreshed.Right - refreshedAt.Length + 1, refreshed.Top, refreshedAt);

        var tips = _geometry.TipsRectangle;
        _console.WriteAt(tips.Left, tips.Top, "Arrow keys / Enter / Esc");
        const string commands = "Ctrl-R Refresh / Ctrl-C Exit";
        _console.WriteAt(tips.Right - commands.Length + 1, tips.Top, commands);
    }

    public void OnExit()
    {
        _console.FillRectangle(_geometry.Full, ' ');
        Console.SetCursorPosition(0,  _geometry.Full.Top + 1);
        Console.WriteLine("Thank you for using kvs-cache!");
        Console.WriteLine();
        Console.CursorVisible = true;
    }
}
