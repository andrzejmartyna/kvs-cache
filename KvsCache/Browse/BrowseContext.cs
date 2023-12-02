using KvsCache.ConsoleDraw;

namespace KvsCache.Browse;

public class BrowseContext
{
    public ConsoleUi Console { get; }
    public CancellationToken CancellationToken { get; private set; }
    public ManualResetEvent RefreshEvent { get; }
    
    private readonly List<Browser> _browsers = new();

    public BrowseContext(ConsoleUi console, ManualResetEvent refreshEvent)
    {
        Console = console;
        RefreshEvent = refreshEvent;
    }

    public Browser this[int index] => _browsers[index];

    public void AddBrowser(Browser browser) => _browsers.Add(browser);

    public void SetCancellationToken(CancellationToken token)
    {
        CancellationToken = token;
    }
}
