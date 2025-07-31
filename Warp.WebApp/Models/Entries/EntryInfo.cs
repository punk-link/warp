using System.Text.Json.Serialization;
using Warp.WebApp.Models.Entries.Enums;
using Warp.WebApp.Models.Images;

namespace Warp.WebApp.Models.Entries;

public readonly record struct EntryInfo
{
    [JsonConstructor]
    public EntryInfo(Guid id, 
        Guid creatorId, 
        DateTime createdAt,
        DateTime expiresAt, 
        EditMode editMode,
        Entry entry, 
        List<ImageInfo> imageInfos,
        EntryOpenGraphDescription openGraphDescription, 
        long viewCount)
    {
        CreatedAt = createdAt;
        CreatorId = creatorId;
        EditMode = editMode;
        Entry = entry;
        ExpiresAt = expiresAt;
        Id = id;
        ImageInfos = imageInfos;
        OpenGraphDescription = openGraphDescription;
        ViewCount = viewCount;
    }

        
    public Guid Id { get; }
    public DateTime CreatedAt { get; }
    public Guid CreatorId { get; }
    public EditMode EditMode { get; } = EditMode.Unset;
    public Entry Entry { get; }
    public DateTime ExpiresAt { get; }
    public List<ImageInfo> ImageInfos { get; init; }
    public EntryOpenGraphDescription OpenGraphDescription { get; }
    public long ViewCount { get; init; }
}