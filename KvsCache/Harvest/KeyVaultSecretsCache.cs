using KvsCache.Models.Azure;
using KvsCache.Models.Errors;
using Newtonsoft.Json;

namespace KvsCache.Harvest;

public class KeyVaultSecretsCache
{
    public string SchemaVersion { get; init; } = "2.0";
    
    public Subscriptions Subscriptions { get; private init; } = new();

    public static KeyVaultSecretsCache ReadFromFile(string filePath)
    {
        var errorInfo = new ErrorInfo("No data found"); 
        try
        {
            if (!File.Exists(filePath))
            {
                return new KeyVaultSecretsCache();
            }
            
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            var deserialized = JsonConvert.DeserializeObject<KeyVaultSecretsCache>(File.ReadAllText(filePath), settings);
            if (deserialized != null)
            {
                return deserialized;
            }
            errorInfo = new ErrorNotFound($"Data in {filePath}");
        }
        catch (Exception e)
        {
            errorInfo = new ErrorInfo(e.Message);
        }

        try
        {
            var backupNumber = 0;
            var backupPath = $"{filePath}.backup{backupNumber:00}.json";
            while (File.Exists(backupPath))
            {
                ++backupNumber;
                backupPath = $"{filePath}.backup{backupNumber}.json";
            }
            File.Copy(filePath, backupPath);
        }
        catch (Exception e)
        {
            //eat it as the goal of this try-catch is to do its best to backup a file
        }

        var cache = new KeyVaultSecretsCache();
        cache.Subscriptions.SetErrorState(errorInfo);
        return cache;
    }

    public OneOrError<bool> WriteCacheToFile(string filePath)
    {
        try
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            File.WriteAllText(filePath, JsonConvert.SerializeObject(this, Formatting.Indented, settings));
            return true;
        }
        catch (Exception e)
        {
            return new ErrorInfo(e.Message);
        }
    }
}
