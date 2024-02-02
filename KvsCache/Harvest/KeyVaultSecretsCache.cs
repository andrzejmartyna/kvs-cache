using KvsCache.Models.Azure;
using KvsCache.Models.Errors;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KvsCache.Harvest;

public class KeyVaultSecretsCache
{
    // SchemaVersion must be in the format of major.minor where major and minor are numbers
    public string SchemaVersion { get; set; } = "3.0";
    
    public Subscriptions Subscriptions { get; private init; } = new();

    public static OneOrError<decimal> ReadSchemaVersionFromFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return 0;
            }
            var parsedObject = JObject.Parse(File.ReadAllText(filePath));
            var token = parsedObject.SelectToken("$.SchemaVersion");
            if (token != null)
            {
                return decimal.Parse(token.ToString());
            }
            return new ErrorInfo($"Error reading SchemaVersion from the {filePath} file");
        }
        catch (Exception e)
        {
            return new ErrorInfo(e.Message);
        }
    }

    public static OneOrError<KeyVaultSecretsCache> ReadFromFile(string filePath)
    {
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
            return new ErrorNotFound($"Data in {filePath}");
        }
        catch (Exception e)
        {
            return new ErrorInfo(e.Message);
        }
    }

    public static OneOrError<string> BackupCacheFile(string filePath)
    {
        string backupPath;
        try
        {
            var backupNumber = 0;
            // TODO: strange thing - string interpolation ended sometimes with NullReferenceException
            // this fails sometimes: $"{filePath}.backup{backupNumber:00}.json";
            backupPath = filePath + ".backup" + backupNumber.ToString("00") + ".json";
            while (File.Exists(backupPath))
            {
                ++backupNumber;
                backupPath = filePath + ".backup" + backupNumber.ToString("00") + ".json";
            }
            File.Copy(filePath, backupPath);
        }
        catch (Exception e)
        {
            return new ErrorInfo(e.Message);
        }

        return backupPath;
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
