using KvsCache.Models.Azure;

namespace KvsCache.Browse;

public record BrowserItem(BrowserItemType ItemType, DataItem? Self, BrowserItem? Parent, string ErrorMessage)
{
    public string DisplayName => Self?.DisplayName ?? ErrorMessage;

    public static IEnumerable<BrowserItem> PackForBrowsing(DataChunk dataChunk, BrowserItem? parent)
    {
        if (dataChunk is { ItemsAvailable: true, Items: not null })
        {
            foreach (var item in dataChunk.Items)
            {
                yield return new BrowserItem(BrowserItemType.Fetched, item, parent, string.Empty);
            }
        }
        else
        {
            yield return new BrowserItem(BrowserItemType.Error, null, parent, dataChunk.LastOperationError?.Message ?? "Unknown error");
        }
    }
}
