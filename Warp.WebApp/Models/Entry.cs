using System.Text.Json.Serialization;

namespace Warp.WebApp.Models;

public readonly record struct Entry
{
    [JsonConstructor]
    public Entry(Guid id, string content, string description, DateTime createdAt, DateTime expiresAt)
    {
        Id = id;
        Content = content;
        Description = description;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }


    public Guid Id { get; }
    public string Content { get; }
    public string Description { get; }
    public DateTime CreatedAt { get; }
    public DateTime ExpiresAt { get; }
}