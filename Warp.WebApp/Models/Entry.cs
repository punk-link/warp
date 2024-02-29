using System.Text.Json.Serialization;

namespace Warp.WebApp.Models;

public readonly record struct Entry
{
    [JsonConstructor]
    public Entry(Guid id, string content, DateTime createdAt, DateTime expiresAt)
    {
        Id = id;
        Content = content;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }


    public Guid Id { get; }
    public string Content { get; }
    public DateTime CreatedAt { get; }
    public DateTime ExpiresAt { get; }
}