using Warp.WebApp.Models.Entries.Enums;
using Warp.WebApp.Models.Images;
using Warp.WebApp.Models.Images.Converters;
using Warp.WebApp.Models.Moderation;
using Warp.WebApp.Models.Moderation.Enums;

namespace Warp.WebApp.Models.Entries;

public readonly record struct EntryApiResponse
{
    public EntryApiResponse(string id, EditMode editMode, ExpirationPeriod expirationPeriod, DateTimeOffset expiresAt, List<ImageInfo> images, List<ImageInfo> excludedImages, string textContent, string? contentDelta, long viewCount, ModerationResult? textModerationResult = null, bool isCreator = false)
    {
        Id = id;
        ContentDelta = contentDelta;
        EditMode = editMode;
        ExcludedImages = excludedImages.ToImageInfoResponse(isCreator);
        ExpirationPeriod = expirationPeriod;
        ExpiresAt = expiresAt;
        Images = images.ToImageInfoResponse(isCreator);
        IsTextBlurred = !isCreator && IsFlaggedByModeration(textModerationResult);
        TextContent = textContent;
        TextModerationResult = textModerationResult;
        ViewCount = viewCount;
    }


    // TODO: save expiration period in the database
    public EntryApiResponse(string id, EntryInfo entryInfo, bool isCreator = false) 
        : this(id, entryInfo.EditMode, ExpirationPeriod.FiveMinutes, entryInfo.ExpiresAt, entryInfo.ImageInfos, entryInfo.ExcludedImageInfos, entryInfo.Entry.Content, entryInfo.Entry.ContentDelta, entryInfo.ViewCount, entryInfo.TextModerationResult, isCreator)
    {
    }


    public static EntryApiResponse Empty(string id) 
        => new(id, EditMode.Unset, ExpirationPeriod.FiveMinutes, DateTimeOffset.MinValue, [], [], string.Empty, null, 0);


    private static bool IsFlaggedByModeration(ModerationResult? result)
        => result is { Status: ModerationStatus.Completed, IsFlagged: true };


    public string Id { get; }
    public string? ContentDelta { get; }
    public EditMode EditMode { get; } = EditMode.Unset;
    public List<ImageInfoResponse> ExcludedImages { get; } = [];
    public ExpirationPeriod ExpirationPeriod { get; } = ExpirationPeriod.FiveMinutes;
    public DateTimeOffset ExpiresAt { get; }
    public List<ImageInfoResponse> Images { get; } = [];
    public bool IsTextBlurred { get; }
    public List<string> RejectedFiles { get; init; } = [];
    public string TextContent { get; } = string.Empty;
    public ModerationResult? TextModerationResult { get; }
    public long ViewCount { get; }
}
