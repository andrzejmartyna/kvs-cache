using KvsCache.Browse;
using KvsCache.ConsoleDraw;
using KvsCache.Models.Azure;
using KvsCache.Models.Errors;

namespace KvsCache.Harvest;

public class Harvester
{
    private readonly string _cacheFile = "kvs-cache.json";
    private readonly KeyVaultSecretsRepository _keyVaultSecretsRepository = new();

    private KeyVaultSecretsCache _cache = new();

    public Subscriptions Subscriptions => _cache.Subscriptions;

    //TODO: Remove dependency of Harvester on UI
    private readonly ConsoleUi _console;
    private readonly BrowseGeometry _geometry;
    
    public int TestSleepInMs { get; set; }

    public Harvester(ConsoleUi console, BrowseGeometry geometry)
    {
        _console = console;
        _geometry = geometry;
    }

    public bool PrepareCache()
    {
        ErrorInfo? error = null;

        var latestSchemaVersion = new KeyVaultSecretsCache().SchemaVersion;

        var fileSchemaVersionOrError = KeyVaultSecretsCache.ReadSchemaVersionFromFile(_cacheFile);
        if (fileSchemaVersionOrError.TryPickT1(out var errorReadingFileSchemaVersion, out var fileSchemaVersion))
        {
            error = errorReadingFileSchemaVersion;
        }
        else
        {
            if (fileSchemaVersion <= 0 || fileSchemaVersion >= decimal.Parse(latestSchemaVersion))
            {
                var cacheOrError = KeyVaultSecretsCache.ReadFromFile(_cacheFile);
                if (cacheOrError.TryPickT1(out var errorReadingCache, out var cache))
                {
                    error = errorReadingCache;
                }
                else
                {
                    _cache = cache;
                    _cache.SchemaVersion = latestSchemaVersion;
                    return true;
                }
            }
        }

        var saveColor = Console.ForegroundColor;

        if (error != null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"ERROR: Error reading the cache file {_cacheFile}.");
            Console.WriteLine($"Error is: {error.Message}");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"WARNING: Schema of the cache file {_cacheFile} is {fileSchemaVersion}.");
            Console.WriteLine("This is incompatible with the current version of kvs-cache.");
        }

        Console.WriteLine("kvs-cache cannot use the file.");
        Console.WriteLine("Backup of the file will be made and the application will start to build its new cache from scratch.");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Press any key to continue");
        Console.ForegroundColor = saveColor;
        Console.ReadKey();

        var okOrError = KeyVaultSecretsCache.BackupCacheFile(_cacheFile);
        if (okOrError.TryPickT1(out var errorBackuping, out var backupFile))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"ERROR: Cannot backup file {_cacheFile}.");
            Console.WriteLine($"Error is: {errorBackuping.Message}");
            Console.WriteLine();
            Console.WriteLine("Backup or remove the file manually and rerun kvs-cache.");
            Console.ForegroundColor = saveColor;
            return false;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Backup file made: {backupFile}");
        Console.WriteLine("Press any key to continue");
        Console.ForegroundColor = saveColor;
        Console.ReadKey();

        return true;
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

        var result = chunk.SetCachedState(list.Cast<DataItem>().ToList());

        //TODO: This should be done asynchronously however for now the bigger issue is that cache is not written until someone exits the application
        //TODO: thus it's better to write it everytime something new was reloaded
        WriteCache();
        return result;
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
