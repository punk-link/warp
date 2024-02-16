namespace Warp.WebApp.Models;

public class EntryInfo
{
    public Entry Entry { get; init; } = default!;
    public List<Guid> ImageIds { get; init; } = default!;
    public long ViewCount { get; init; }
}