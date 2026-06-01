using Warp.WebApp.Models.Moderation;
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


    public ImageInfoResponse(in ImageInfo imageInfo, bool isCreator = false) : this(imageInfo.Id, imageInfo.EntryId, imageInfo.Url)
    {
        ModerationResult = imageInfo.ModerationResult;
        IsBlurred = !isCreator && IsFlaggedByModeration(imageInfo.ModerationResult);
    }


    public string Id { get; init; }
    public string EntryId { get; init; }
    public bool IsBlurred { get; init; }
    public ModerationResult? ModerationResult { get; init; }
    public Uri Url { get; init; }


    private static bool IsFlaggedByModeration(ModerationResult? result)
        => result is { Status: ModerationStatus.Completed, IsFlagged: true };
}
