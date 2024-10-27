using Microsoft.AspNetCore.Http.HttpResults;
using System.Text.Json.Serialization;
using Warp.WebApp.Models.Entries;
using Warp.WebApp.Models.Entries.Enums;

namespace Warp.WebApp.Models;

public readonly record struct EntryInfo
{
    [JsonConstructor]
    public EntryInfo(Guid id, 
        Guid creatorId, 
        DateTime createdAt,
        DateTime expiresAt, 
        EditMode editMode,
        Entry entry, 
        EntryOpenGraphDescription openGraphDescription, 
        long viewCount)
    {
        CreatedAt = createdAt;
        CreatorId = creatorId;
        EditMode = editMode;
        Entry = entry;
        ExpiresAt = expiresAt;
        Id = id;
        OpenGraphDescription = openGraphDescription;
        ViewCount = viewCount;
    }


    public EntryInfo(in Guid id, 
        in Guid creatorId, 
        in DateTime createdAt, 
        in DateTime expiresAt,
        EditMode editMode,
        in Entry entry, 
        in EntryOpenGraphDescription openGraphDescription)
        : this(id, creatorId, createdAt, expiresAt, editMode, entry, openGraphDescription, 0)
    {
    }

        
    public Guid Id { get; }
    public DateTime CreatedAt { get; }
    public Guid CreatorId { get; }
    public EditMode EditMode { get; } = EditMode.Unknown;
    public Entry Entry { get; }
    public DateTime ExpiresAt { get; }
    public EntryOpenGraphDescription OpenGraphDescription { get; }
    public long ViewCount { get; init; }
}