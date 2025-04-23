using Warp.WebApp.Constants.Logging;
using Warp.WebApp.Extensions;

namespace Warp.WebApp.Models.Errors;

public readonly record struct DomainError
{
    public DomainError(LogEvents code, string? detail = null)
    {
        Code = code;
        Detail = detail ?? code.ToDescriptionString();
    }
    

    public DomainError WithExtension(string key, object value)
    {
        Extensions[key] = value;
        return this;
    }


    public LogEvents Code { get; }
    public string Detail { get; }
    public Dictionary<string, object> Extensions { get; init; } = [];
}
