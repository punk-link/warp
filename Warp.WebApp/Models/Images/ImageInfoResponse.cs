using Warp.WebApp.Services;

namespace Warp.WebApp.Models.Images;

public readonly record struct ImageInfoResponse
{
    public ImageInfoResponse(in Guid id, in Guid entryId, Uri url)
    {
        Id = IdCoder.Encode(id);
        EntryId = IdCoder.Encode(entryId);
        Url = url;
    }


    public ImageInfoResponse(in ImageInfo imageInfo): this(imageInfo.Id, imageInfo.EntryId, imageInfo.Url)
    {
    }


    public string Id { get; init; }
    public string EntryId { get; init; }
    public Uri Url { get; init; }
}
