using KvsCache.Models.Azure;
using Newtonsoft.Json;

namespace KvsCache.Models.Errors;

public class ErrorInfo : DataItem
{
    [JsonProperty]
    public string Message { get; private set; }
    
    public ErrorInfo(string message)
    {
        Message = message;
    }

    public override string DisplayName => "Error: " + (Message.Split(Environment.NewLine).FirstOrDefault() ?? "Unknown error");
}
