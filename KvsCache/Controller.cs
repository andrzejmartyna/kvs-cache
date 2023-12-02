using System.Reflection;
using KvsCache.Browse;
using KvsCache.ConsoleDraw;
using KvsCache.KeyVaults;
using KvsCache.Models.Azure;
using KvsCache.Models.Errors;
using KvsCache.Models.Geometry;
using Newtonsoft.Json;

namespace KvsCache;

public class Controller
{
    private string _cacheFile = "kvs-cache.json";
    private TimeSpan _cacheMaxAge = new TimeSpan(1, 0, 0, 0);

    private KeyVaultSecretsCache? _currentCache;
    private readonly ConsoleUi _console;
    private readonly BrowseGeometry _geometry;
    private readonly KeyVaultSecretsRepository _keyVaultSecretsRepository = new();
    private readonly ManualResetEvent _breakPressed = new(false);
    private int _testSleepInfo;
    private int _testSleepSecret;

    public Controller(Rectangle operationRectangle)
    {
        _geometry = new BrowseGeometry(operationRectangle);
        _console = new ConsoleUi(_geometry);
    }

    private void CacheOrReadSecrets() => ReadingSecrets(_cacheMaxAge);
    private void RereadSecrets() => ReadingSecrets(new TimeSpan());

    private void ReadingSecrets(TimeSpan maxAge)
    {
        var cacheOrError = KeyVaultSecretsCache.ObtainValidCache(_cacheFile, maxAge, _keyVaultSecretsRepository);
        if (cacheOrError.TryPickT0(out var cache, out var error))
        {
            _currentCache = cache;
        }
        //TODO: not to loose error
        if (_testSleepInfo > 0)
        {
            Thread.Sleep(_testSleepInfo);
        }
    }

    public void Break()
    {
        _breakPressed.Set();
    }
    
    public void Execute(int testSleepInfo, int testSleepSecret)
    {
        _testSleepInfo = testSleepInfo;
        _testSleepSecret = testSleepSecret;
        
        Console.CursorVisible = false;
        
        InitialDraw();
        
        _currentCache = KeyVaultSecretsCache.ReadFromFile(_cacheFile);
        if (_currentCache == null)
        {
            Progress.Run(CacheOrReadSecrets, _console, _geometry.RefreshedRectangle, "Collecting information");
        }

        BrowseSubscriptions();

        OnExit();
    }

    private void BrowseSubscriptions()
    {
        if (_currentCache == null) return;
        
        var refreshEvent = new ManualResetEvent(false);
        var browsingContext = new BrowseContext(_console, refreshEvent);
        browsingContext.AddBrowser(new Browser(browsingContext, BrowseKeyVaults, null, "Subscriptions", false));
        browsingContext.AddBrowser(new Browser(browsingContext, BrowseSecrets, null, "KeyVaults", true));
        browsingContext.AddBrowser(new Browser(browsingContext, ReadSecretValue, InfoSecretValue, "Secrets", true));
        
        while (true)
        {
            DrawStatistics();
            
            if (!_currentCache.IsValidAge(_cacheMaxAge))
            {
                refreshEvent.Set();
            }

            var cts = new CancellationTokenSource();
            browsingContext.SetCancelationToken(cts.Token);

            var browsingTcs = new TaskCompletionSource<bool>();
            var browsingTask = Task.Run(() =>
            {
                //TODO: cache it!
                browsingContext[0].Browse(BrowserItem.PackForBrowsing(_keyVaultSecretsRepository.GetSubscriptions(), null), null);
                browsingTcs.SetResult(true);
            }, browsingContext.CancellationToken);

            var  browsingWaitHandle = ((IAsyncResult)browsingTcs.Task).AsyncWaitHandle;
            var exitOrRefresh = WaitHandle.WaitAny(new[] { browsingWaitHandle, _breakPressed, refreshEvent });
            if (exitOrRefresh <= 1)
            {
                if (exitOrRefresh == 1)
                {
                    cts.Cancel();
                    browsingTask.Wait();
                }
                break;
            }
            
            var refreshTcs = new TaskCompletionSource<bool>();
            var refreshTask = Task.Run(() =>
            {
                Progress.Run(RereadSecrets, _console, _geometry.RefreshedRectangle, "Reading");
                refreshTcs.SetResult(true);
            }, browsingContext.CancellationToken);

            var  refreshWaitHandle = ((IAsyncResult)refreshTcs.Task).AsyncWaitHandle;
            var exitOrRefreshed = WaitHandle.WaitAny(new[] { browsingWaitHandle, _breakPressed, refreshWaitHandle });
            cts.Cancel();
            if (exitOrRefreshed <= 1)
            {
                if (exitOrRefreshed == 1)
                {
                    browsingTask.Wait();
                }
                refreshTask.Wait();
                break;
            }

            refreshEvent.Reset();
            browsingTask.Wait();
        }
    }

    private void BrowseKeyVaults(BrowserItem selected, BrowseContext context)
    {
        if (_currentCache == null) return;
        
        var selection = _geometry.SelectionRectangle;
        _console.WriteAt(selection.Left, selection.Top, selected.DisplayName);
        
        //TODO: cache it!
        context[1].Browse(BrowserItem.PackForBrowsing(_keyVaultSecretsRepository.GetKeyVaults(selected.Self), selected), selected);
        
        _console.WriteAt(selection.Left, selection.Top, new string(' ', selection.Width));
    }

    private void BrowseSecrets(BrowserItem selected, BrowseContext context)
    {
        if (_currentCache == null) return;

        var selection = _geometry.SelectionRectangle;
        _console.WriteAt(selection.Left, selection.Top + 1, selected.DisplayName);

        if (selected.Self is KeyVault kv)
        {
            //TODO: cache it!
            context[2].Browse(BrowserItem.PackForBrowsing(_keyVaultSecretsRepository.GetSecrets(kv.Url), selected), selected);
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
        var keyVault = (KeyVault?)selected.Parent?.Children?.FirstOrDefault()?.Self;
        var secret = (Secret?)selected.Children?.FirstOrDefault()?.Self;
        if (keyVault == null || secret == null)
        {
            _console.Message($"Internal error - no KeyVault found for the {secret?.Name} secret", _console.RedMessage);
            return;
        }

        if (info)
        {
            var subscription = (Subscription?)selected.Parent?.Parent?.Children?.FirstOrDefault()?.Self;
            var secretInfo = new SecretFullInfo(
                new SubscriptionInfo(subscription?.Id, subscription?.Name),
                new KeyVaultInfo(keyVault.Name, keyVault.Url),
                new SecretInfo(secret.Name));
            Clipboard.SetText(JsonConvert.SerializeObject(secretInfo, Formatting.Indented));
            _console.Message("The clipboard was filled with full information about the secret.", _console.GreenMessage);
            return;
        }
        
        OneOrError<string> secretValueOrError = string.Empty;
        Progress.Run(() =>
        {
            secretValueOrError = _keyVaultSecretsRepository.GetSecretValue(keyVault.Url, secret.Name);
            if (_testSleepSecret > 0)
            {
                Thread.Sleep(_testSleepSecret);
            }
        }, _console, _geometry.ReadingProgressRectangle, "Reading");

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
        string version = $"kvs-cache v{Assembly.GetExecutingAssembly().GetName().Version?.ToString(2) ?? "..."}";
        _console.WriteAt(versionInfo.Right - version.Length + 1, versionInfo.Top, version);
    }

    private void DrawStatistics()
    {
        if (_currentCache == null) return;

        var info = _geometry.SummaryRectangle;
        _console.WriteAt(info.Left, info.Top + 0, $"Subscriptions: {_currentCache.SubscriptionCount}");
        _console.WriteAt(info.Left, info.Top + 1, $"   Key vaults: {_currentCache.KeyVaultCount}");
        _console.WriteAt(info.Left, info.Top + 2, $"      Secrets: {_currentCache.SecretCount}");

        var refreshed = _geometry.RefreshedRectangle;
        var refreshedAt = $"Refreshed at {_currentCache.CachedAt}";
        _console.WriteAt(refreshed.Right - refreshedAt.Length + 1, refreshed.Top, refreshedAt);

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
