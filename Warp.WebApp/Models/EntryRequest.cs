using Warp.WebApp.Models.Entries.Enums;

namespace Warp.WebApp.Models;

public readonly record struct EntryRequest
{
    public EditMode EditMode { get; init; }
    public TimeSpan ExpiresIn { get; init; }
    public List<Guid> ImageIds { get; init; }
    public string TextContent { get; init; }
}
