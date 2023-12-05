using KvsCache.Browse;
using KvsCache.ConsoleDraw;
using KvsCache.Models.Azure;
using KvsCache.Models.Errors;

namespace KvsCache.Harvest;

public class Harvester
{
    //TODO: private readonly string _cacheFile = "kvs-cache.json";
    private readonly KeyVaultSecretsRepository _keyVaultSecretsRepository = new();

    private readonly Subscriptions _cache = new();

    //TODO: Remove dependency of Harvester on UI
    private readonly ConsoleUi _console;
    private readonly BrowseGeometry _geometry;
    
    public int TestSleepInMs { get; set; }

    public Harvester(ConsoleUi console, BrowseGeometry geometry)
    {
        _console = console;
        _geometry = geometry;
    }

    public string SubscriptionCount => _cache.Items?.Count.ToString() ?? "?";
    public string KeyVaultCount => _cache.Items?.Sum(s => ((Subscription)s).Items?.Count ?? 0).ToString() ?? "?";
    public string SecretCount => _cache.Items?.Sum(s => ((Subscription)s).Items?.Sum(kv => ((KeyVault)kv).Items?.Count ?? 0)).ToString() ?? "?";

    public DataChunk GetSubscriptions(bool forceRefresh)
        => GetCachedOrReadDataChunk(_cache, () => _keyVaultSecretsRepository.GetSubscriptions(), forceRefresh);

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
