using System.Text.Json.Serialization;
using Warp.WebApp.Models.Entries.Enums;

namespace Warp.WebApp.Models;

public readonly record struct Entry
{
    [JsonConstructor]
    public Entry(Guid id, string content, string description, DateTime createdAt, DateTime expiresAt, EditMode editMode)
    {
        Id = id;
        Content = content;
        Description = description;
        CreatedAt = createdAt;
        EditMode = editMode;
        ExpiresAt = expiresAt;
    }


    public Guid Id { get; }
    public string Content { get; }
    public string Description { get; }
    public DateTime CreatedAt { get; }
    public EditMode EditMode { get; } = EditMode.Unknown;
    public DateTime ExpiresAt { get; }
}