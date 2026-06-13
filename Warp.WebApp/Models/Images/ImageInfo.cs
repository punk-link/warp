using System.Text.Json.Serialization;
using Warp.WebApp.Models.Moderation;

namespace Warp.WebApp.Models.Images;

public readonly record struct ImageInfo
{
    [JsonConstructor]
    public ImageInfo(Guid id, Guid entryId, Uri url)
    {
        Id = id;
        EntryId = entryId;
        Url = url;
    }


    public Guid Id { get; init; }
    public Guid EntryId { get; init; }
    public ModerationResult? ModerationResult { get; init; }
    public Uri Url { get; init; }
}
