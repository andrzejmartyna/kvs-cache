using System.Collections;
using KvsCache.Models.Errors;

namespace KvsCache.Browse;

public record BrowserItem(BrowserItemType ItemType, dynamic? Self, dynamic[]? Children, BrowserItem? Parent, string ErrorMessage, DateTime CachedAt)
{
    public string DisplayName => Self != null ? Self.Name : ErrorMessage;

    public static IEnumerable<BrowserItem> PackForBrowsing<T>(OneOrError<List<T>> itemsOrError, BrowserItem?parent)
    {
        if (itemsOrError.TryPickT1(out var error, out var list))
        {
            yield return new BrowserItem(BrowserItemType.Error, null, null, parent, error.Message, DateTime.Now);
        }
        else
        {
            foreach (var item in list)
            {
                yield return new BrowserItem(BrowserItemType.Fetched, item, null, parent, string.Empty, DateTime.Now);
            }
        }
    }
}
