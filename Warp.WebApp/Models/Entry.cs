using System.Text.Json.Serialization;
using Warp.WebApp.Models.Entries;
using Warp.WebApp.Models.Entries.Enums;

namespace Warp.WebApp.Models;

public readonly record struct Entry
{
    [JsonConstructor]
    public Entry(Guid id, string content, DateTime createdAt, DateTime expiresAt, EditMode editMode, List<Uri> imageUrls,
        EntryOpenGraphDescription openGraphDescription)
    {
        Id = id;
        Content = content;
        CreatedAt = createdAt;
        EditMode = editMode;
        ExpiresAt = expiresAt;
        ImageUrls = imageUrls;
        OpenGraphDescription = openGraphDescription;
    }


    public Entry(Guid id, string content, DateTime createdAt, DateTime expiresAt, EditMode editMode) 
        : this(id, content, createdAt, expiresAt, editMode, [], EntryOpenGraphDescription.Empty)
    {
    }


    public Guid Id { get; }
    public string Content { get; }
    public DateTime CreatedAt { get; }
    public EditMode EditMode { get; } = EditMode.Unknown;
    public DateTime ExpiresAt { get; }
    public List<Uri> ImageUrls { get; init; }
    public EntryOpenGraphDescription OpenGraphDescription { get; init; }
}