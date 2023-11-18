namespace kcs_cache.Browse;

public record BrowserItem(BrowserItemType ItemType, string DisplayName, object[] Items, BrowserItem? Parent);
