using System.Text.Json.Serialization;

namespace Warp.WebApp.Models.Entries;

public readonly record struct Entry
{
    [JsonConstructor]
    public Entry(string content, string? contentDelta = null)
    {
        Content = content;
        ContentDelta = contentDelta;
    }


    public string Content { get; }
    public string? ContentDelta { get; }
}