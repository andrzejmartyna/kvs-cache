using KvsCache.Models.Errors;

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
        var itemsList = items.ToList();
        var errors = itemsList.Where(a => a.Self is ErrorInfo);
        var itemSorted = new SortedDictionary<string, BrowserItem>(itemsList.Where(a => a.Self is not ErrorInfo).ToDictionary(b => b.DisplayName, b => b));

        Items = new List<BrowserItem>();
        foreach (var error in errors)
        {
            Items.Add(new BrowserItem(BrowserItemType.Fetched, error.Self, parentItem));
        }
        foreach (var item in itemSorted)
        {
            Items.Add(new BrowserItem(BrowserItemType.Fetched, item.Value.Self, parentItem));
        }
    }
}
