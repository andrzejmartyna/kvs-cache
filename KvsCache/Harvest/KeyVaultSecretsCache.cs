using KvsCache.Models.Azure;
using KvsCache.Models.Errors;
using Newtonsoft.Json;

namespace KvsCache.Harvest;

public class KeyVaultSecretsCache
{
    public string SchemaVersion { get; init; } = "2.0";
    
    public Subscriptions Subscriptions { get; private init; } = new();

    public static OneOrError<KeyVaultSecretsCache> ReadFromFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
                var deserialized = JsonConvert.DeserializeObject<KeyVaultSecretsCache>(File.ReadAllText(filePath), settings);
                if (deserialized != null)
                {
                    return deserialized;
                }
                return new ErrorNotFound($"Data in {filePath}");
            }
            return new ErrorNotFound($"File {filePath}");
        }
        catch (Exception e)
        {
            return new ErrorInfo(e.Message);
        }
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
