using System.Text.Json.Serialization;
using Warp.WebApp.Models.Entries.Enums;
using Warp.WebApp.Models.Images;

namespace Warp.WebApp.Models.Entries;

public readonly record struct EntryInfo
{
    [JsonConstructor]
    public EntryInfo(Guid id, 
        Guid creatorId, 
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt, 
        EditMode editMode,
        Entry entry, 
        List<ImageInfo> imageInfos,
        long viewCount)
    {
        CreatedAt = createdAt;
        CreatorId = creatorId;
        EditMode = editMode;
        Entry = entry;
        ExpiresAt = expiresAt;
        Id = id;
        ImageInfos = imageInfos;
        ViewCount = viewCount;
    }

        
    public Guid Id { get; }
    public DateTimeOffset CreatedAt { get; }
    public Guid CreatorId { get; }
    public EditMode EditMode { get; } = EditMode.Unset;
    public Entry Entry { get; }
    public DateTimeOffset ExpiresAt { get; }
    public List<ImageInfo> ImageInfos { get; init; }
    public long ViewCount { get; init; }
}