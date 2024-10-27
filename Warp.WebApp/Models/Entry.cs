using System.Text.Json.Serialization;

namespace Warp.WebApp.Models;

public readonly record struct Entry
{
    [JsonConstructor]
    public Entry(string content, List<Guid> imageIds)
    {
        Content = content;
        ImageIds = imageIds;
    }


    public Entry(string content) 
        : this(content, [])
    {
    }


    public string Content { get; }
    public List<Guid> ImageIds { get; init; }
}