namespace KvsCache.Browse;

public class BrowseState
{
    public string Filter { get; }
    
    public int Count => Items.Count;
    
    public BrowserSelection Selection { get; private set; } = new(0, 0);

    public List<BrowserItem> Items { get; private set; } = new();

    public BrowseState(IEnumerable<BrowserItem> items, BrowserItem? parentItem, string filter)
    {
        Filter = filter;
        ResetItems(items, parentItem);
    }

    public BrowserItem this[int index] => Items[index];
    public BrowserItem Selected => this[Selection.Selected];

    public void SetSelection(int firstDisplayed, int selected)
    {
        Selection = new BrowserSelection(firstDisplayed, selected);
    }

    public void ResetItems(IEnumerable<BrowserItem> items, BrowserItem? parentItem)
    {
        var itemSorted = new SortedDictionary<string, BrowserItem>(items.ToDictionary(a => a.DisplayName, a => a));

        Items = new List<BrowserItem>();
        foreach (var item in itemSorted)
        {
            Items.Add(new BrowserItem(BrowserItemType.Fetched, item.Value.Self, parentItem, string.Empty));
        }
    }
}
