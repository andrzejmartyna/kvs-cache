namespace KvsCache.Browse;

public record BrowserItem(BrowserItemType ItemType, dynamic? Self, dynamic[]? Children, BrowserItem? Parent, string ErrorMessage, DateTime CachedAt)
{
    public string DisplayName => Self != null ? Self.Name : ErrorMessage;
}
