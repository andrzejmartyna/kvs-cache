using KvsCache.Models.Azure;

namespace KvsCache.Harvest;

public static class CachePolicies
{
    public static bool IsCacheValid(DataChunk? chunk)
    {
        if (chunk == null) return false;
        
        var span = new TimeSpan(1, 0, 0, 0);
        switch (chunk)
        {
            case Subscriptions:
                span = new TimeSpan(30, 0, 0, 0);
                break;
            case Subscription:
                span = new TimeSpan(7, 0, 0, 0);
                break;
            case KeyVault:
                span = new TimeSpan(1, 0, 0, 0);
                break;
        }

        return chunk.CachedAt + span > DateTime.Now;
    }
}
