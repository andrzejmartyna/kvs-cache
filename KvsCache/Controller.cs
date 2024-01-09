using System.Reflection;
using System.Text.RegularExpressions;
using KvsCache.Browse;
using KvsCache.ConsoleDraw;
using KvsCache.Harvest;
using KvsCache.Models.Azure;
using KvsCache.Models.Errors;
using KvsCache.Models.Geometry;
using KvsCache.Utils;
using Newtonsoft.Json;
using TextCopy;

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
            var parent = new BrowserItem(BrowserItemType.Fetched, _harvester.Subscriptions, null);
            browsingContext[0].Browse((forceRefresh) => _harvester.GetSubscriptions(forceRefresh), parent, DrawStatistics);
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
        if (WasError(selected))
        {
            return;
        }
        
        var selection = _geometry.SelectionRectangle;
        _console.WriteAt(selection.Left, selection.Top, selected.DisplayName);

        if (selected.Self is Subscription subscription)
        {
            context[1].Browse((forceRefresh) => _harvester.GetKeyVaults(subscription, forceRefresh), selected, DrawStatistics);
        }

        _console.WriteAt(selection.Left, selection.Top, new string(' ', selection.Width));
    }

    private bool WasError(BrowserItem selected)
    {
        if (selected.Self is ErrorInfo err)
        {
            _console.Message("There was an error", err.Message, _console.RedMessage);
            return true;
        }
        return false;
    }

    private void BrowseSecrets(BrowserItem selected, BrowseContext context)
    {
        if (WasError(selected))
        {
            return;
        }
        
        var selection = _geometry.SelectionRectangle;
        _console.WriteAt(selection.Left, selection.Top + 1, selected.DisplayName);

        if (selected.Self is KeyVault kv)
        {
            context[2].Browse((forceRefresh) => _harvester.GetSecrets(kv, forceRefresh), selected, DrawStatistics);
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
        if (WasError(selected))
        {
            return;
        }
        
        var secret = selected.Self as Secret;
        if (secret == null || selected.Parent?.Self is not KeyVault keyVault)
        {
            _console.Message("Internal error", $"No KeyVault found for the {secret?.Name} secret", _console.RedMessage);
            return;
        }

        if (info)
        {
            var subscription = selected.Parent?.Parent?.Self as Subscription;
            var secretInfo = new SecretFullInfo(
                new SubscriptionInfo(subscription?.Id, subscription?.Name),
                new KeyVaultInfo(keyVault.Name, keyVault.Url),
                new SecretInfo(secret.Name));
            ClipboardService.SetText(JsonConvert.SerializeObject(secretInfo, Formatting.Indented));
            _console.Message("Secret value", "The clipboard was filled with full information about the secret.", _console.GreenMessage);
            return;
        }
        
        var secretValueOrError = _harvester.GetSecretValue(keyVault, secret.Name);
        secretValueOrError.Switch(
            str =>
            {
                var testForBase64 = Regex.IsMatch(str, @"^[a-zA-Z0-9\+/\s]*=*$");
                var decodedFromBase64 = string.Empty;

                if (testForBase64)
                {
                    var cleaned = Regex.Replace(str, @"\s+", string.Empty);
                    cleaned = cleaned.PadRight(cleaned.Length / 4 * 4 + (cleaned.Length % 4 == 0 ? 0 : 4), '=');

                    var buffer = new Span<byte>(new byte[cleaned.Length]);
                    testForBase64 = Convert.TryFromBase64String(cleaned, buffer, out var bytesParsed);
                    if (testForBase64)
                    {
                        decodedFromBase64 = System.Text.Encoding.UTF8.GetString(buffer[..bytesParsed]);
                    }
                }

                var message = "Value of the secret was copied to the clipboard.";
                if (testForBase64)
                {
                    message += Environment.NewLine + "It's base64 encoded. Press any key to decode.";
                }
                
                ClipboardService.SetText(str);
                _console.Message("Secret value", message, _console.GreenMessage);
                
                if (testForBase64)
                {
                    ClipboardService.SetText(decodedFromBase64);
                    _console.Message("Secret value", "Value decoded from base64 was copied to the clipboard.", _console.GreenMessage);
                }
                
            },
            err => _console.Message("Error getting secret value", err.Message, _console.RedMessage)
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

        var cacheInfo = _geometry.RefreshedRectangle;
        _console.FillRectangle(cacheInfo, ' ');
        if (cachedAt != null)
        {
            var cachedAtString = DateTimeFormat.FormatTimeAgo(cachedAt.Value);
            _console.WriteAt(cacheInfo.Right - cachedAtString.Length + 1, cacheInfo.Top, cachedAtString);
        }
    }

    private void OnExit()
    {
        _console.FillRectangle(_geometry.Full, ' ');
        Console.SetCursorPosition(0,  _geometry.Full.Top + 1);
        Console.WriteLine("Thank you for using kvs-cache!");
        Console.WriteLine();
        Console.CursorVisible = true;
        
        _harvester.WriteCache();
    }
}
