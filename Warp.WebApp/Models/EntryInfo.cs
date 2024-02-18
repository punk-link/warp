namespace Warp.WebApp.Models;

public readonly record struct EntryInfo
{
    public EntryInfo(Entry entry, long viewCount, List<Guid> imageIds)
    {
        Entry = entry;
        ViewCount = viewCount;
        ImageIds = imageIds;
    }

    public Entry Entry { get; init; }
    public List<Guid> ImageIds { get; init; }
    public long ViewCount { get; init; }
}