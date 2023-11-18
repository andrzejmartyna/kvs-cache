namespace kcs_cache.Browse;

public class BrowseState
{
    public string Filter { get; }

    public int Count => _items.Count;
    
    public bool Entered { get; set; }
    
    public BrowserSelection Selection { get; private set; } = new(0, 0);
    
    private List<BrowserItem> _items = new();

    public BrowseState(IEnumerable<(string, object)> items, BrowserItem? parentItem, string filter)
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

    public void ResetItems(IEnumerable<(string, object)> items, BrowserItem? parentItem)
    {
        var itemSorted = new SortedDictionary<string, object>(items.ToDictionary(a => a.Item1, a => a.Item2));

        _items = new List<BrowserItem>();
        foreach (var item in itemSorted)
        {
            _items.Add(new BrowserItem(BrowserItemType.Single, item.Key, new [] { item.Value }, parentItem));
        }
    }
}
