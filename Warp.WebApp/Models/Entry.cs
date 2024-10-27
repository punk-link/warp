using System.Text.Json.Serialization;
using Warp.WebApp.Models.Entries.Enums;

namespace Warp.WebApp.Models;

public readonly record struct Entry
{
    [JsonConstructor]
    public Entry(Guid id, string content, DateTime createdAt, DateTime expiresAt, EditMode editMode, List<Guid> imageIds)
    {
        Id = id;
        Content = content;
        CreatedAt = createdAt;
        EditMode = editMode;
        ExpiresAt = expiresAt;
        ImageIds = imageIds;
    }


    public Entry(Guid id, string content, DateTime createdAt, DateTime expiresAt, EditMode editMode) 
        : this(id, content, createdAt, expiresAt, editMode, [])
    {
    }


    public Guid Id { get; }
    public string Content { get; }
    public DateTime CreatedAt { get; }
    public EditMode EditMode { get; } = EditMode.Unknown;
    public DateTime ExpiresAt { get; }
    public List<Guid> ImageIds { get; init; }
}