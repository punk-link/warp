using System.Text.Json.Serialization;
using Warp.WebApp.Models.Entries;

namespace Warp.WebApp.Models;

public readonly record struct EntryInfo
{
    [JsonConstructor]
    public EntryInfo(Guid creatorId, Entry entry, EntryOpenGraphDescription openGraphDescription, long viewCount)
    {
        CreatorId = creatorId;
        Entry = entry;
        OpenGraphDescription = openGraphDescription;
        ViewCount = viewCount;
    }


    public EntryInfo(in Guid creatorId, in Entry entry, in EntryOpenGraphDescription openGraphDescription)
        : this(creatorId, entry, openGraphDescription, 0)
    {
    }


    public Guid CreatorId { get; }
    public Entry Entry { get; }
    public EntryOpenGraphDescription OpenGraphDescription { get; }
    public long ViewCount { get; init; }
}