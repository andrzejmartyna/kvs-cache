namespace KvsCache.Browse;

public class BrowseState
{
    public string Filter { get; }

    public int Count => _items.Count;
    
    public BrowserSelection Selection { get; private set; } = new(0, 0);
    
    private List<BrowserItem> _items = new();

    public BrowseState(IEnumerable<BrowserItem> items, BrowserItem? parentItem, string filter)
    {
        Filter = filter;
        ResetItems(items, parentItem);
    }

    public BrowserItem this[int index] => _items[index];
    public BrowserItem Selected => this[Selection.Selected];

    public void SetSelection(int firstDisplayed, int selected)
    {
        Selection = new BrowserSelection(firstDisplayed, selected);
    }

    public void ResetItems(IEnumerable<BrowserItem> items, BrowserItem? parentItem)
    {
        var itemSorted = new SortedDictionary<string, BrowserItem>(items.ToDictionary(a => a.DisplayName, a => a));

        _items = new List<BrowserItem>();
        foreach (var item in itemSorted)
        {
            _items.Add(new BrowserItem(BrowserItemType.Fetched, item.Value.Self, new dynamic[] { item.Value }, parentItem, string.Empty, DateTime.MinValue));
        }
    }
}
