using Warp.WebApp.Models.Entries;
using Warp.WebApp.Models.Entries.Enums;
using Warp.WebApp.Models.Images;
using Warp.WebApp.Models.Moderation;

namespace Warp.WebApp.Tests.UnitTests.EntryApiResponseTests;

public class EntryApiResponseBlurTests
{
    [Fact]
    public void IsTextBlurred_ShouldBeTrue_WhenNonCreatorAndTextCompletedFlagged()
    {
        var entryInfo = BuildEntryInfo(ModerationResult.CreateCompleted(isFlagged: true, categoryScores: null));

        var response = new EntryApiResponse(id: "id", entryInfo: entryInfo, isCreator: false);

        Assert.True(response.IsTextBlurred);
    }


    [Fact]
    public void IsTextBlurred_ShouldBeFalse_WhenCreatorAndTextCompletedFlagged()
    {
        var entryInfo = BuildEntryInfo(ModerationResult.CreateCompleted(isFlagged: true, categoryScores: null));

        var response = new EntryApiResponse(id: "id", entryInfo: entryInfo, isCreator: true);

        Assert.False(response.IsTextBlurred);
    }


    [Fact]
    public void IsTextBlurred_ShouldBeFalse_WhenNonCreatorAndTextPending()
    {
        var entryInfo = BuildEntryInfo(ModerationResult.CreatePending());

        var response = new EntryApiResponse(id: "id", entryInfo: entryInfo, isCreator: false);

        Assert.False(response.IsTextBlurred);
    }


    [Fact]
    public void IsTextBlurred_ShouldBeFalse_WhenNonCreatorAndTextFailed()
    {
        var entryInfo = BuildEntryInfo(ModerationResult.CreateFailed());

        var response = new EntryApiResponse(id: "id", entryInfo: entryInfo, isCreator: false);

        Assert.False(response.IsTextBlurred);
    }


    [Fact]
    public void IsTextBlurred_ShouldBeFalse_WhenNonCreatorAndTextCompletedUnflagged()
    {
        var entryInfo = BuildEntryInfo(ModerationResult.CreateCompleted(isFlagged: false, categoryScores: null));

        var response = new EntryApiResponse(id: "id", entryInfo: entryInfo, isCreator: false);

        Assert.False(response.IsTextBlurred);
    }


    [Fact]
    public void IsTextBlurred_ShouldBeFalse_WhenNonCreatorAndNoModerationResult()
    {
        var entryInfo = BuildEntryInfo(textModerationResult: null);

        var response = new EntryApiResponse(id: "id", entryInfo: entryInfo, isCreator: false);

        Assert.False(response.IsTextBlurred);
    }


    [Fact]
    public void Images_ShouldHaveIsBlurredTrue_WhenNonCreatorAndImageCompletedFlagged()
    {
        var imageInfo = BuildImageInfo(ModerationResult.CreateCompleted(isFlagged: true, categoryScores: null));
        var entryInfo = BuildEntryInfo(imageInfos: [imageInfo]);

        var response = new EntryApiResponse(id: "id", entryInfo: entryInfo, isCreator: false);

        Assert.True(response.Images[0].IsBlurred);
    }


    [Fact]
    public void Images_ShouldHaveIsBlurredFalse_WhenCreatorAndImageCompletedFlagged()
    {
        var imageInfo = BuildImageInfo(ModerationResult.CreateCompleted(isFlagged: true, categoryScores: null));
        var entryInfo = BuildEntryInfo(imageInfos: [imageInfo]);

        var response = new EntryApiResponse(id: "id", entryInfo: entryInfo, isCreator: true);

        Assert.False(response.Images[0].IsBlurred);
    }


    [Fact]
    public void Images_ShouldHaveIsBlurredFalse_WhenNonCreatorAndImagePending()
    {
        var imageInfo = BuildImageInfo(ModerationResult.CreatePending());
        var entryInfo = BuildEntryInfo(imageInfos: [imageInfo]);

        var response = new EntryApiResponse(id: "id", entryInfo: entryInfo, isCreator: false);

        Assert.False(response.Images[0].IsBlurred);
    }


    [Fact]
    public void Images_ShouldHaveIsBlurredFalse_WhenNonCreatorAndNoModerationResult()
    {
        var imageInfo = BuildImageInfo(moderationResult: null);
        var entryInfo = BuildEntryInfo(imageInfos: [imageInfo]);

        var response = new EntryApiResponse(id: "id", entryInfo: entryInfo, isCreator: false);

        Assert.False(response.Images[0].IsBlurred);
    }


    private static EntryInfo BuildEntryInfo(ModerationResult? textModerationResult = null, List<ImageInfo>? imageInfos = null)
        => new EntryInfo(
            id: Guid.NewGuid(),
            creatorId: Guid.NewGuid(),
            createdAt: DateTimeOffset.UtcNow,
            expiresAt: DateTimeOffset.UtcNow.AddHours(1),
            editMode: EditMode.Simple,
            entry: new Entry("Test content"),
            imageInfos: imageInfos ?? [],
            viewCount: 0)
        with { TextModerationResult = textModerationResult };


    private static ImageInfo BuildImageInfo(ModerationResult? moderationResult = null)
        => new ImageInfo(id: Guid.NewGuid(), entryId: Guid.NewGuid(), url: new Uri("https://example.com/img.png"))
        with { ModerationResult = moderationResult };
}
