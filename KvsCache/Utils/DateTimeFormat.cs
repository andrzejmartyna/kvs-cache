namespace KvsCache.Utils;

public static class DateTimeFormat
{
    public static string FormatTimeAgo(DateTime time, string prefix)
    {
        var span = DateTime.Now - time;
        return span switch
        {
            _ when span < new TimeSpan() => $"{prefix} future",
            _ when span < new TimeSpan(0, 0, 10, 0) => $"{prefix} <10 min ago",
            _ when span < new TimeSpan(0, 1, 0, 0) => $"{prefix} <1h ago",
            _ when span > new TimeSpan(1, 0, 0, 0) => $"{prefix} ~{span.Days} days ago",
            _ => $"{prefix} ~{span.Hours}h ago"
        };
    }
}
