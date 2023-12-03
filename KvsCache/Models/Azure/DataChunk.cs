using KvsCache.Models.Errors;

namespace KvsCache.Models.Azure;

public abstract class DataChunk : DataItem
{
    public bool ItemsAvailable { get; private set; }

    public DateTime CachedAt { get; private set; }

    public DateTime ErroredAt { get; private set; }
    public ErrorInfo? LastOperationError { get; private set; } = new NotLoadedYet();

    public List<DataItem>? Items { get; private set; }

    public DataChunk SetErrorState(ErrorInfo error)
    {
        ErroredAt = DateTime.Now;
        LastOperationError = error;
        return this;
    }
    
    public DataChunk SetCachedState(List<DataItem> list)
    {
        ItemsAvailable = true;
        CachedAt = DateTime.Now;
        ErroredAt = default;
        LastOperationError = null;
        Items = list;
        return this;
    }
}
