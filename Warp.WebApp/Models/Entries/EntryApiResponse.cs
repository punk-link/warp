using Warp.WebApp.Models.Entries.Enums;
using Warp.WebApp.Models.Images;
using Warp.WebApp.Models.Images.Converters;
using Warp.WebApp.Services;

namespace Warp.WebApp.Models.Entries;

public readonly record struct EntryApiResponse
{
    public EntryApiResponse(string id, EditMode editMode, ExpirationPeriod expirationPeriod, DateTime expiresAt, List<ImageInfo> images, EntryOpenGraphDescription openGraphDescription, string textContent, long viewCount)
    {
        Id = id;
        EditMode = editMode;
        ExpirationPeriod = expirationPeriod;
        ExpiresAt = expiresAt;
        Images = images.ToImageInfoResponse();
        OpenGraphDescription = openGraphDescription;
        // TODO: move out from the struct
        TextContent = TextFormatter.GetCleanString(textContent);
        ViewCount = viewCount;
    }


    // TODO: save expiration period in the database
    public EntryApiResponse(string id, EntryInfo entryInfo) 
        : this(id, entryInfo.EditMode, ExpirationPeriod.FiveMinutes, entryInfo.ExpiresAt, entryInfo.ImageInfos, entryInfo.OpenGraphDescription, entryInfo.Entry.Content, entryInfo.ViewCount)
    {
    }


    public static EntryApiResponse Empty(string id, EntryOpenGraphDescription description) 
        => new (id, EditMode.Unset, ExpirationPeriod.FiveMinutes, DateTime.MinValue, [], description, string.Empty, 0);


    public string Id { get; }
    public EditMode EditMode { get; } = EditMode.Unset;
    public ExpirationPeriod ExpirationPeriod { get; } = ExpirationPeriod.FiveMinutes;
    public DateTime ExpiresAt { get; }
    public List<ImageInfoResponse> Images { get; } = [];
    public EntryOpenGraphDescription OpenGraphDescription { get; } = default!;
    public string TextContent { get; } = string.Empty;
    public long ViewCount { get; }
}
