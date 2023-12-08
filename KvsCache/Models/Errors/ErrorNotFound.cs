using Newtonsoft.Json;

namespace KvsCache.Models.Errors;

public class ErrorNotFound : ErrorInfo
{
    public ErrorNotFound(string className) : base($"{className} Not Found")
    {
        this.ClassName = className;
    }

    [JsonProperty]
    public string ClassName { get; init; }
}
