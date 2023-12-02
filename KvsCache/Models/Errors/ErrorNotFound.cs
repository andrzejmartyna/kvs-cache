namespace KvsCache.Models.Errors;

public record ErrorNotFound(string ClassName) : ErrorInfo($"{ClassName} Not Found")
{
    public string ClassName { get; init; } = ClassName;
}
