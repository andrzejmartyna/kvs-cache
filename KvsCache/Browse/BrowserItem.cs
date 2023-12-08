using KvsCache.Models.Azure;

namespace KvsCache.Browse;

public record BrowserItem(BrowserItemType ItemType, DataItem? Self, BrowserItem? Parent)
{
    public string DisplayName => Self?.DisplayName ?? "Unknown error";

    public static IEnumerable<BrowserItem> PackForBrowsing(DataChunk dataChunk, BrowserItem? parent)
    {
        if (dataChunk.Items == null || dataChunk.LastOperationError != null)
        {
            yield return new BrowserItem(BrowserItemType.Error, dataChunk.LastOperationError, parent);
        }
        if (dataChunk.Items != null)
        {
            foreach (var item in dataChunk.Items)
            {
                yield return new BrowserItem(BrowserItemType.Fetched, item, parent);
            }
        }
    }
}
