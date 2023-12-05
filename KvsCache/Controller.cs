using System.Reflection;
using KvsCache.Browse;
using KvsCache.ConsoleDraw;
using KvsCache.Harvest;
using KvsCache.Models.Azure;
using KvsCache.Models.Geometry;
using Newtonsoft.Json;

namespace KvsCache;

public class Controller
{
    private readonly ConsoleUi _console;
    private readonly BrowseGeometry _geometry;
    private readonly Harvester _harvester;
    private readonly ManualResetEvent _breakPressed = new(false);

    public Controller(Rectangle operationRectangle)
    {
        _geometry = new BrowseGeometry(operationRectangle);
        _console = new ConsoleUi(_geometry);
        _harvester = new Harvester(_console, _geometry);
    }

    public void Break()
    {
        _breakPressed.Set();
    }
    
    public void Execute(int testSleepInMs)
    {
        _harvester.TestSleepInMs = testSleepInMs;
        
        Console.CursorVisible = false;
        
        InitialDraw();
        
        BrowseSubscriptions();

        OnExit();
    }

    private void BrowseSubscriptions()
    {
        var browsingContext = new BrowseContext(_console);
        browsingContext.AddBrowser(new Browser(browsingContext, BrowseKeyVaults, null, "Subscriptions", false));
        browsingContext.AddBrowser(new Browser(browsingContext, BrowseSecrets, null, "KeyVaults", true));
        browsingContext.AddBrowser(new Browser(browsingContext, ReadSecretValue, InfoSecretValue, "Secrets", true));
        
        DrawStatistics();
        
        var cts = new CancellationTokenSource();
        browsingContext.SetCancellationToken(cts.Token);

        var browsingTcs = new TaskCompletionSource<bool>();
        var browsingTask = Task.Run(() =>
        {
            browsingContext[0].Browse((forceRefresh) => BrowserItem.PackForBrowsing(_harvester.GetSubscriptions(forceRefresh), null), null, DrawStatistics);
            browsingTcs.SetResult(true);
        }, browsingContext.CancellationToken);

        var  browsingWaitHandle = ((IAsyncResult)browsingTcs.Task).AsyncWaitHandle;
        var exitWays = WaitHandle.WaitAny(new[] { browsingWaitHandle, _breakPressed });
        if (exitWays == 1)
        {
            cts.Cancel();
            browsingTask.Wait();
        }
    }

    private void BrowseKeyVaults(BrowserItem selected, BrowseContext context)
    {
        var selection = _geometry.SelectionRectangle;
        _console.WriteAt(selection.Left, selection.Top, selected.DisplayName);

        if (selected.Self is Subscription subscription)
        {
            context[1].Browse((forceRefresh) => BrowserItem.PackForBrowsing(_harvester.GetKeyVaults(subscription, forceRefresh), selected), selected, DrawStatistics);
        }

        _console.WriteAt(selection.Left, selection.Top, new string(' ', selection.Width));
    }

    private void BrowseSecrets(BrowserItem selected, BrowseContext context)
    {
        var selection = _geometry.SelectionRectangle;
        _console.WriteAt(selection.Left, selection.Top + 1, selected.DisplayName);

        if (selected.Self is KeyVault kv)
        {
            context[2].Browse((forceRefresh) => BrowserItem.PackForBrowsing(_harvester.GetSecrets(kv, forceRefresh), selected), selected, DrawStatistics);
        }
        
        _console.WriteAt(selection.Left, selection.Top + 1, new string(' ', selection.Width));
    }

    private void InfoSecretValue(BrowserItem selected)
    {
        InfoOrReadSecretValue(selected, true);
    }

    private void ReadSecretValue(BrowserItem selected, BrowseContext context)
    {
        InfoOrReadSecretValue(selected, false);
    }
    
    private void InfoOrReadSecretValue(BrowserItem selected, bool info)
    {
        var secret = selected.Self as Secret;
        if (secret == null || selected.Parent?.Self is not KeyVault keyVault)
        {
            _console.Message($"Internal error - no KeyVault found for the {secret?.Name} secret", _console.RedMessage);
            return;
        }

        if (info)
        {
            var subscription = selected.Parent?.Parent?.Self as Subscription;
            var secretInfo = new SecretFullInfo(
                new SubscriptionInfo(subscription?.Id, subscription?.Name),
                new KeyVaultInfo(keyVault.Name, keyVault.Url),
                new SecretInfo(secret.Name));
            Clipboard.SetText(JsonConvert.SerializeObject(secretInfo, Formatting.Indented));
            _console.Message("The clipboard was filled with full information about the secret.", _console.GreenMessage);
            return;
        }
        
        var secretValueOrError = _harvester.GetSecretValue(keyVault, secret.Name);
        secretValueOrError.Switch(
            str =>
            {
                Clipboard.SetText(str);
                _console.Message("Value of the secret was copied to the clipboard.", _console.GreenMessage);
            },
            err => _console.Message($"There was an error getting the secret value.\r\n{err.Message}", _console.RedMessage)
        );
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
        var version = $"kvs-cache v{Assembly.GetExecutingAssembly().GetName().Version?.ToString(2) ?? "..."}";
        _console.WriteAt(versionInfo.Right - version.Length + 1, versionInfo.Top, version);
    }

    private void DrawStatistics(DateTime? cachedAt = null)
    {
        var info = _geometry.SummaryRectangle;
        _console.FillRectangle(info, ' ');
        _console.WriteAt(info.Left, info.Top + 0, $" Subscriptions: {_harvester.SubscriptionCount}");
        _console.WriteAt(info.Left, info.Top + 1, $"    KVs cached: {_harvester.KeyVaultCount}");
        _console.WriteAt(info.Left, info.Top + 2, $"Secrets cached: {_harvester.SecretCount}");

        var tips = _geometry.TipsRectangle;
        _console.WriteAt(tips.Left, tips.Top, "Arrow keys / Enter / Esc");
        const string commands = "Ctrl-R Refresh / Ctrl-C Exit";
        _console.WriteAt(tips.Right - commands.Length + 1, tips.Top, commands);
    }

    private void OnExit()
    {
        _console.FillRectangle(_geometry.Full, ' ');
        Console.SetCursorPosition(0,  _geometry.Full.Top + 1);
        Console.WriteLine("Thank you for using kvs-cache!");
        Console.WriteLine();
        Console.CursorVisible = true;
    }
}
