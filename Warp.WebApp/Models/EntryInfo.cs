using System.Text.Json.Serialization;

namespace Warp.WebApp.Models;

public readonly record struct EntryInfo
{
    [JsonConstructor]
    public EntryInfo(Entry entry, long viewCount, List<Guid> imageIds)
    {
        Entry = entry;
        ViewCount = viewCount;
        ImageIds = imageIds;
    }


    public Entry Entry { get; }
    public List<Guid> ImageIds { get; }
    public long ViewCount { get; }
}