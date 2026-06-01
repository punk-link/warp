using Warp.WebApp.Models.Entries.Enums;
using Warp.WebApp.Models.Images;
using Warp.WebApp.Models.Images.Converters;
using Warp.WebApp.Models.Moderation;

namespace Warp.WebApp.Models.Entries;

public readonly record struct EntryApiResponse
{
    public EntryApiResponse(string id, EditMode editMode, ExpirationPeriod expirationPeriod, DateTimeOffset expiresAt, List<ImageInfo> images, List<ImageInfo> excludedImages, string textContent, string? contentDelta, long viewCount, ModerationResult? textModerationResult = null, bool isCreator = false)
    {
        Id = id;
        EditMode = editMode;
        ExpirationPeriod = expirationPeriod;
        ExpiresAt = expiresAt;
        Images = images.ToImageInfoResponse(isCreator);
        ExcludedImages = excludedImages.ToImageInfoResponse(isCreator);
        TextContent = textContent;
        ContentDelta = contentDelta;
        ViewCount = viewCount;
        TextModerationResult = textModerationResult;
        IsTextBlurred = !isCreator && IsFlaggedByModeration(textModerationResult);
    }


    // TODO: save expiration period in the database
    public EntryApiResponse(string id, EntryInfo entryInfo, bool isCreator = false) 
        : this(id, entryInfo.EditMode, ExpirationPeriod.FiveMinutes, entryInfo.ExpiresAt, entryInfo.ImageInfos, entryInfo.ExcludedImageInfos, entryInfo.Entry.Content, entryInfo.Entry.ContentDelta, entryInfo.ViewCount, entryInfo.TextModerationResult, isCreator)
    {
    }


    public static EntryApiResponse Empty(string id) 
        => new(id, EditMode.Unset, ExpirationPeriod.FiveMinutes, DateTimeOffset.MinValue, [], [], string.Empty, null, 0);


    public string Id { get; }
    public EditMode EditMode { get; } = EditMode.Unset;
    public ExpirationPeriod ExpirationPeriod { get; } = ExpirationPeriod.FiveMinutes;
    public DateTimeOffset ExpiresAt { get; }
    public List<ImageInfoResponse> Images { get; } = [];
    public bool IsTextBlurred { get; }
    public List<ImageInfoResponse> ExcludedImages { get; } = [];
    public List<string> RejectedFiles { get; init; } = [];
    public string TextContent { get; } = string.Empty;
    public ModerationResult? TextModerationResult { get; }
    public string? ContentDelta { get; }
    public long ViewCount { get; }


    private static bool IsFlaggedByModeration(ModerationResult? result)
        => result is { Status: ModerationStatus.Completed, IsFlagged: true };
}
