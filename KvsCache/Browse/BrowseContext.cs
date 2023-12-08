using KvsCache.ConsoleDraw;

namespace KvsCache.Browse;

public class BrowseContext
{
    public ConsoleUi Console { get; }
    public CancellationToken CancellationToken { get; private set; }
    
    private readonly List<Browser> _browsers = new();

    public BrowseContext(ConsoleUi console)
    {
        Console = console;
    }

    public Browser this[int index] => _browsers[index];

    public void AddBrowser(Browser browser) => _browsers.Add(browser);

    public void SetCancellationToken(CancellationToken token)
    {
        CancellationToken = token;
    }
}
