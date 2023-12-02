using OneOf;

namespace KvsCache.Models.Errors;

public class OneOrError<T> : OneOfBase<T, ErrorInfo>
{
    private OneOrError(OneOf<T, ErrorInfo> _) : base(_) { }
    public static implicit operator OneOrError<T>(T _) => new(_);
    public static implicit operator OneOrError<T>(ErrorInfo _) => new(_);

    //TODO: do something about dynamic
    public string? Name => Match(item =>  item != null ? ((dynamic)item).Name : string.Empty, err => err.Message);
}
