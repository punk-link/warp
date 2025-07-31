using System.Text.Json.Serialization;

namespace Warp.WebApp.Models.Entries;

public readonly record struct Entry
{
    [JsonConstructor]
    public Entry(string content)
    {
        Content = content;
    }


    public string Content { get; }
}