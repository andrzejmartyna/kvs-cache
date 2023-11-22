using KvsCache.ConsoleDraw;

namespace KvsCache.Browse;

public class BrowseContext
{
    public ConsoleUi Console { get; init; }
    public CancellationToken CancellationToken { get; private set; }
    public ManualResetEvent RefreshEvent { get; init; }
    
    private readonly List<Browser> _browsers = new();

    public BrowseContext(ConsoleUi console, ManualResetEvent refreshEvent)
    {
        Console = console;
        RefreshEvent = refreshEvent;
    }

    public Browser this[int index] => _browsers[index];

    public void AddBrowser(Browser browser) => _browsers.Add(browser);

    public void SetCancelationToken(CancellationToken token)
    {
        CancellationToken = token;
    }
}
