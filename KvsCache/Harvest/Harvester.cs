using KvsCache.Browse;
using KvsCache.ConsoleDraw;
using KvsCache.Models.Azure;
using KvsCache.Models.Errors;

namespace KvsCache.Harvest;

public class Harvester
{
    private readonly string _cacheFile = "kvs-cache.json";
    private readonly KeyVaultSecretsRepository _keyVaultSecretsRepository = new();

    private readonly KeyVaultSecretsCache _cache = new();

    //TODO: Remove dependency of Harvester on UI
    private readonly ConsoleUi _console;
    private readonly BrowseGeometry _geometry;
    
    public int TestSleepInMs { get; set; }

    public Harvester(ConsoleUi console, BrowseGeometry geometry)
    {
        _console = console;
        _geometry = geometry;
        
        var tryRead = KeyVaultSecretsCache.ReadFromFile(_cacheFile);
        if (tryRead.TryPickT0(out var cache, out var _))
        {
            _cache = cache;
        }
    }

    public void WriteCache()
    {
        _cache.WriteCacheToFile(_cacheFile);
    }

    public string SubscriptionCount => _cache.Subscriptions.Items?.Count.ToString() ?? "?";
    public string KeyVaultCount => _cache.Subscriptions.Items?.Sum(s => ((Subscription)s).Items?.Count ?? 0).ToString() ?? "?";
    public string SecretCount => _cache.Subscriptions.Items?.Sum(s => ((Subscription)s).Items?.Sum(kv => ((KeyVault)kv).Items?.Count ?? 0)).ToString() ?? "?";

    public DataChunk GetSubscriptions(bool forceRefresh)
        => GetCachedOrReadDataChunk(_cache.Subscriptions, () => _keyVaultSecretsRepository.GetSubscriptions(), forceRefresh);

    public DataChunk GetKeyVaults(Subscription subscription, bool forceRefresh)
        => GetCachedOrReadDataChunk(subscription, () => _keyVaultSecretsRepository.GetKeyVaults(subscription.Name), forceRefresh);

    public DataChunk GetSecrets(KeyVault keyVault, bool forceRefresh)
        => GetCachedOrReadDataChunk(keyVault, () => _keyVaultSecretsRepository.GetSecrets(keyVault.Url), forceRefresh);

    public OneOrError<string> GetSecretValue(KeyVault keyVault, string secretName)
        => RunBlockingOperationWithProgress(() =>  _keyVaultSecretsRepository.GetSecretValue(keyVault.Url, secretName));

    private DataChunk GetCachedOrReadDataChunk<T>(DataChunk chunk, Func<OneOrError<List<T>>> readItemsFunction, bool forceRefresh)
    {
        if (!forceRefresh && CachePolicies.IsCacheValid(chunk))
        {
            return chunk;
        }

        var keyVaultsOrError = RunBlockingOperationWithProgress(readItemsFunction);
        if (keyVaultsOrError.TryPickT1(out var error, out var list))
        {
            return chunk.SetErrorState(error);
        }
        return chunk.SetCachedState(list.Cast<DataItem>().ToList());
    }
    
    //TODO: Remove dependency of Harvester on UI
    private OneOrError<T> RunBlockingOperationWithProgress<T>(Func<OneOrError<T>> function)
    {
        OneOrError<T> resultOrError = new ErrorInfo("Execution failed");
        Progress.Run(() =>
        {
            resultOrError = function();
            if (TestSleepInMs > 0)
            {
                Thread.Sleep(TestSleepInMs);
            }
        }, _console, _geometry.ReadingProgressRectangle, "Reading");
        return resultOrError;
    }
}
