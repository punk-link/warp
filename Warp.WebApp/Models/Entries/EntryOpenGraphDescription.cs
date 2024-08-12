using System.Text.Json.Serialization;

namespace Warp.WebApp.Models.Entries;

public readonly record struct EntryOpenGraphDescription
{
    [JsonConstructor]
    public EntryOpenGraphDescription(string title, string description, Uri? imageUrl)
    {
        Description = description;
        ImageUrl = imageUrl;
        Title = title;
    }


    public static EntryOpenGraphDescription Empty 
        => new(string.Empty, string.Empty, null);


    public string Title { get; }
    public string Description { get; }
    public Uri? ImageUrl { get; }
}
