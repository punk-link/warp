using Warp.WebApp.Models.Entries.Enums;
using Warp.WebApp.Models.Images;
using Warp.WebApp.Models.Images.Converters;

namespace Warp.WebApp.Models.Entries;

public readonly record struct EntryApiResponse
{
    public EntryApiResponse(string id, EditMode editMode, ExpirationPeriod expirationPeriod, DateTimeOffset expiresAt, List<ImageInfo> images, string textContent, string? contentDelta, long viewCount)
    {
        Id = id;
        EditMode = editMode;
        ExpirationPeriod = expirationPeriod;
        ExpiresAt = expiresAt;
        Images = images.ToImageInfoResponse();
        TextContent = textContent;
        ContentDelta = contentDelta;
        ViewCount = viewCount;
    }


    // TODO: save expiration period in the database
    public EntryApiResponse(string id, EntryInfo entryInfo) 
        : this(id, entryInfo.EditMode, ExpirationPeriod.FiveMinutes, entryInfo.ExpiresAt, entryInfo.ImageInfos, entryInfo.Entry.Content, entryInfo.Entry.ContentDelta, entryInfo.ViewCount)
    {
    }


    public static EntryApiResponse Empty(string id) 
        => new (id, EditMode.Unset, ExpirationPeriod.FiveMinutes, DateTimeOffset.MinValue, [], string.Empty, null, 0);


    public string Id { get; }
    public EditMode EditMode { get; } = EditMode.Unset;
    public ExpirationPeriod ExpirationPeriod { get; } = ExpirationPeriod.FiveMinutes;
    public DateTimeOffset ExpiresAt { get; }
    public List<ImageInfoResponse> Images { get; } = [];
    public string TextContent { get; } = string.Empty;
    public string? ContentDelta { get; }
    public long ViewCount { get; }
}
