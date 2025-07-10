using Warp.WebApp.Models.Entries.Enums;

namespace Warp.WebApp.Models.Entries;

public readonly record struct EntryApiRequest
{
    public EditMode EditMode { get; init; }
    public ExpirationPeriod ExpirationPeriod { get; init; }
    public List<string> ImageIds { get; init; } 
    public string TextContent { get; init; }
}
