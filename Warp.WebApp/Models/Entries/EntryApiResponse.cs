using Warp.WebApp.Models.Entries.Enums;
using Warp.WebApp.Models.Images;
using Warp.WebApp.Models.Images.Converters;
using Warp.WebApp.Services;

namespace Warp.WebApp.Models.Entries;

public readonly record struct EntryApiResponse
{
    public EntryApiResponse(string id, EditMode editMode, ExpirationPeriod expirationPeriod, List<ImageInfo> images, EntryOpenGraphDescription openGraphDescription, string textContent)
    {
        Id = id;
        EditMode = editMode;
        ExpirationPeriod = expirationPeriod;
        Images = images.ToImageInfoResponse();
        OpenGraphDescription = openGraphDescription;
        // TODO: move out from the struct
        TextContent = TextFormatter.GetCleanString(textContent);
    }


    public EntryApiResponse(string id, EntryInfo entryInfo) : this(id, entryInfo.EditMode, ExpirationPeriod.FiveMinutes, entryInfo.ImageInfos, entryInfo.OpenGraphDescription, entryInfo.Entry.Content)
    {
    }


    public static EntryApiResponse Empty(string id, EntryOpenGraphDescription description) 
        => new (id, EditMode.Unset, ExpirationPeriod.FiveMinutes, [], description!, string.Empty);


    public string Id { get; }
    public EditMode EditMode { get; } = EditMode.Unset;
    public ExpirationPeriod ExpirationPeriod { get; } = ExpirationPeriod.FiveMinutes;
    public List<ImageInfoResponse> Images { get; } = [];
    public EntryOpenGraphDescription OpenGraphDescription { get; } = default!;
    public string TextContent { get; } = string.Empty;
}
