using KvsCache.Models.Azure;

namespace KvsCache.Browse;

public record BrowserItem(BrowserItemType ItemType, dynamic? Self, dynamic[]? Children, BrowserItem? Parent, string ErrorMessage, DateTime CachedAt)
{
    public string DisplayName => Self != null ? Self.Name : ErrorMessage;

    public static IEnumerable<BrowserItem> PackForBrowsing(DataChunk dataChunk, BrowserItem? parent)
    {
        if (dataChunk is { ItemsAvailable: true, Items: not null })
        {
            foreach (var item in dataChunk.Items)
            {
                yield return new BrowserItem(BrowserItemType.Fetched, item, null, parent, string.Empty, DateTime.Now);
            }
        }
        else
        {
            yield return new BrowserItem(BrowserItemType.Error, null, null, parent, dataChunk.LastOperationError?.Message ?? "Unknown error", DateTime.Now);
        }
    }
}
