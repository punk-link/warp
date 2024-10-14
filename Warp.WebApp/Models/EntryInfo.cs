using System.Text.Json.Serialization;

namespace Warp.WebApp.Models;

public readonly record struct EntryInfo
{
    [JsonConstructor]
    public EntryInfo(Entry entry, long viewCount, List<Guid> imageIds)
    {
        Entry = entry;
        ViewCount = viewCount;
    }


    public Entry Entry { get; }
    public long ViewCount { get; }
}