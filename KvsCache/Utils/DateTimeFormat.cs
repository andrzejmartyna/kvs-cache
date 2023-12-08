namespace KvsCache.Utils;

public static class DateTimeFormat
{
    public static string FormatTimeAgo(DateTime time)
    {
        var span = DateTime.Now - time;
        return span switch
        {
            _ when span < new TimeSpan() => "Wrong cached date",
            _ when span < new TimeSpan(0, 0, 10, 0) => "Cached <10 min ago",
            _ when span < new TimeSpan(0, 1, 0, 0) => "Cached <1h ago",
            _ when span < new TimeSpan(2, 0, 0, 0) => $"Cached ~{span.Hours}h ago",
            _ when span < new TimeSpan(400, 0, 0, 0) => $"Cached ~{span.Days} days ago",
            _ => "No data"
        };
    }
}
