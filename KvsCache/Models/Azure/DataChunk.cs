using KvsCache.Models.Errors;
using Newtonsoft.Json;

namespace KvsCache.Models.Azure;

public abstract class DataChunk : DataItem
{
    [JsonProperty]
    public DateTime CachedAt { get; private set; }

    [JsonProperty]
    public DateTime ErroredAt { get; private set; }
    
    [JsonProperty]
    public ErrorInfo? LastOperationError { get; private set; } = new NotLoadedYet();

    [JsonProperty]
    public List<DataItem>? Items { get; private set; }

    public DataChunk SetErrorState(ErrorInfo error)
    {
        ErroredAt = DateTime.Now;
        LastOperationError = error;
        return this;
    }
    
    public DataChunk SetCachedState(List<DataItem> list)
    {
        CachedAt = DateTime.Now;
        ErroredAt = default;
        LastOperationError = null;
        Items = list;
        return this;
    }

    public void SetTo(DataChunk cloneFrom)
    {
        CachedAt = cloneFrom.CachedAt;
        ErroredAt = cloneFrom.ErroredAt;
        LastOperationError = cloneFrom.LastOperationError;
        Items = cloneFrom.Items;
    }
}
