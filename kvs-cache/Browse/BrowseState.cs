namespace kcs_cache.Browse;

public class BrowseState
{
    public string Filter { get; }

    public int Count => _items.Count;
    
    public BrowserSelection Selection { get; private set; } = new BrowserSelection(0, 0);

    private readonly List<BrowserItem> _items = new List<BrowserItem>();
    private string _currentFilter = string.Empty;

    public CancellationToken CancellationToken { get; init; }
    public ManualResetEvent RefreshEvent { get; init; }

    public BrowseState(IEnumerable<(string, object)> items, BrowserItem? parentItem, string filter, CancellationToken cancellationToken, ManualResetEvent refreshEvent)
    {
        Filter = filter;
        CancellationToken = cancellationToken;
        RefreshEvent = refreshEvent;
        
        var itemSorted = new SortedDictionary<string, object>(items.ToDictionary(a => a.Item1, a => a.Item2));

        foreach (var item in itemSorted)
        {
            _items.Add(new BrowserItem(BrowserItemType.Single, item.Key, new object[] { item.Value }, parentItem));
        }
    }

    public BrowserItem this[int index] => _items[index];

    public void SetSelection(int firstDisplayed, int selected)
    {
        Selection = new BrowserSelection(firstDisplayed, selected);
    }
}
