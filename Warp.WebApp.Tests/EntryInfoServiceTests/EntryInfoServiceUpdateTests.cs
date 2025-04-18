using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using Warp.WebApp.Data;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Creators;
using Warp.WebApp.Models.Entries;
using Warp.WebApp.Models.Entries.Enums;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Creators;
using Warp.WebApp.Services.Entries;
using Warp.WebApp.Services.Images;
using Warp.WebApp.Services.OpenGraph;

namespace Warp.WebApp.Tests.EntryInfoServiceTests;

public class EntryInfoServiceUpdateTests
{
    public EntryInfoServiceUpdateTests()
    {
        _entryInfoService = new EntryInfoService(
            _creatorServiceSubstitute,
            _dataStorageSubstitute,
            _entryServiceSubstitute,
            _imageServiceSubstitute,
            _loggerFactorySubstitute,
            _openGraphServiceSubstitute,
            _reportServiceSubstitute,
            _localizerSubstitute,
            _viewCountServiceSubstitute
        );
        _creator = new Creator(Guid.NewGuid());
    }


    [Fact]
    public async Task Update_ShouldSucceed_WhenEntryBelongsToCreatorAndViewCountIsZero()
    {
        var entryId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        var entryRequest = new EntryRequest
        {
            Id = entryId,
            ExpiresIn = TimeSpan.FromDays(1),
            EditMode = EditMode.Advanced,
            TextContent = "Updated Text",
            ImageIds = new List<Guid> { Guid.NewGuid() }
        };

        var existingEntry = new Entry("Original content");
        var updatedEntry = new Entry("Updated Text");
        
        var imageInfos = new List<ImageInfo>
        {
            new() { Id = Guid.NewGuid(), Url = new Uri("http://example.com/image.jpg") }
        };
        
        var existingEntryInfo = new EntryInfo(entryId, _creator.Id, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 
            EditMode.Advanced, existingEntry, new List<ImageInfo>(), new EntryOpenGraphDescription("Original", "Original", null), 0);
        
        var updatedEntryInfo = new EntryInfo(entryId, _creator.Id, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 
            EditMode.Advanced, updatedEntry, imageInfos, new EntryOpenGraphDescription("Updated", "Updated", imageInfos[0].Url), 0);

        var entryInfoCacheKey = CacheKeyBuilder.BuildEntryInfoCacheKey(entryId);
        _dataStorageSubstitute.TryGet<EntryInfo?>(entryInfoCacheKey, cancellationToken)
            .Returns(new ValueTask<EntryInfo?>(existingEntryInfo));

        _viewCountServiceSubstitute.Get(entryId, cancellationToken)
            .Returns(Task.FromResult(0L));

        _entryServiceSubstitute.Add(Arg.Any<EntryRequest>(), cancellationToken)
            .Returns(Result.Success<Entry, ProblemDetails>(updatedEntry));

        _imageServiceSubstitute.GetAttached(entryId, entryRequest.ImageIds, cancellationToken)
            .Returns(Result.Success<List<ImageInfo>, ProblemDetails>(imageInfos));

        var imageUrl = imageInfos.Select(x => x.Url).FirstOrDefault();
        _openGraphServiceSubstitute.BuildDescription(updatedEntry.Content, imageUrl)
            .Returns(new EntryOpenGraphDescription("Updated", "Updated", imageUrl));

        _creatorServiceSubstitute.AttachEntry(_creator, Arg.Any<EntryInfo>(), cancellationToken)
            .Returns(Result.Success<EntryInfo, ProblemDetails>(updatedEntryInfo));

        _dataStorageSubstitute.Set(Arg.Any<string>(), Arg.Any<EntryInfo>(), Arg.Any<TimeSpan>(), cancellationToken)
            .Returns(Result.Success());
        
        _localizerSubstitute["EntryExpirationPeriodEmptyErrorMessage"]
            .Returns(new LocalizedString("EntryExpirationPeriodEmptyErrorMessage", "Entry period is empty."));

        var result = await _entryInfoService.Update(_creator, entryRequest, cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(updatedEntry.Content, result.Value.Entry.Content);
        Assert.Equal(_creator.Id, result.Value.CreatorId);
        Assert.Equal(imageInfos.Count, result.Value.ImageInfos.Count);
        
        await _viewCountServiceSubstitute.Received(1).Get(entryId, cancellationToken);
    }


    [Fact]
    public async Task Update_ShouldReturnProblemDetails_WhenEntryDoesNotBelongToCreator()
    {
        var entryId = Guid.NewGuid();
        var differentCreatorId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        var entryRequest = new EntryRequest
        {
            Id = entryId,
            ExpiresIn = TimeSpan.FromDays(1),
            EditMode = EditMode.Simple,
            TextContent = "Updated Text"
        };

        var existingEntryInfo = new EntryInfo(entryId, differentCreatorId, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 
            EditMode.Simple, new Entry("Original content"), new List<ImageInfo>(), new EntryOpenGraphDescription("Original", "Original", null), 0);

        var entryInfoCacheKey = CacheKeyBuilder.BuildEntryInfoCacheKey(entryId);
        _dataStorageSubstitute.TryGet<EntryInfo?>(entryInfoCacheKey, cancellationToken)
            .Returns(new ValueTask<EntryInfo?>(existingEntryInfo));

        var localizedString = new LocalizedString("NoPermissionErrorMessage", "Entry does not belong to creator.");
        _localizerSubstitute["NoPermissionErrorMessage"]
            .Returns(localizedString);

        var result = await _entryInfoService.Update(_creator, entryRequest, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(localizedString.Value, result.Error.Detail);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.Error.Status);
        
        await _entryServiceSubstitute.DidNotReceive().Add(Arg.Any<EntryRequest>(), Arg.Any<CancellationToken>());
    }


    [Fact]
    public async Task Update_ShouldReturnProblemDetails_WhenViewCountIsGreaterThanZero()
    {
        var entryId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        var entryRequest = new EntryRequest
        {
            Id = entryId,
            ExpiresIn = TimeSpan.FromDays(1),
            EditMode = EditMode.Simple,
            TextContent = "Updated Text"
        };

        var existingEntryInfo = new EntryInfo(entryId, _creator.Id, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 
            EditMode.Simple, new Entry("Original content"), new List<ImageInfo>(), new EntryOpenGraphDescription("Original", "Original", null), 0);

        var entryInfoCacheKey = CacheKeyBuilder.BuildEntryInfoCacheKey(entryId);
        _dataStorageSubstitute.TryGet<EntryInfo?>(entryInfoCacheKey, cancellationToken)
            .Returns(new ValueTask<EntryInfo?>(existingEntryInfo));
            
        _viewCountServiceSubstitute.Get(entryId, cancellationToken)
            .Returns(Task.FromResult(5L));

        var localizedString = new LocalizedString("EntryCannotBeEditedAfterViewed", "Entry cannot be edited after being viewed.");
        _localizerSubstitute["EntryCannotBeEditedAfterViewed"]
            .Returns(localizedString);

        var result = await _entryInfoService.Update(_creator, entryRequest, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(localizedString.Value, result.Error.Detail);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.Error.Status);
        
        await _viewCountServiceSubstitute.Received(1).Get(entryId, cancellationToken);
        await _entryServiceSubstitute.DidNotReceive().Add(Arg.Any<EntryRequest>(), Arg.Any<CancellationToken>());
    }


    [Fact]
    public async Task Update_ShouldReturnProblemDetails_WhenEntryDoesNotExist()
    {
        var entryId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        var entryRequest = new EntryRequest
        {
            Id = entryId,
            ExpiresIn = TimeSpan.FromDays(1),
            EditMode = EditMode.Simple,
            TextContent = "Updated Text"
        };

        var entryInfoCacheKey = CacheKeyBuilder.BuildEntryInfoCacheKey(entryId);
        _dataStorageSubstitute.TryGet<EntryInfo?>(entryInfoCacheKey, cancellationToken)
            .Returns(new ValueTask<EntryInfo?>((EntryInfo?)null));

        _localizerSubstitute["NotFoundErrorMessage"]
            .Returns(new LocalizedString("NotFoundErrorMessage", "Entry not found."));

        var result = await _entryInfoService.Update(_creator, entryRequest, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal((int)HttpStatusCode.NotFound, result.Error.Status);
        
        await _entryServiceSubstitute.DidNotReceive().Add(Arg.Any<EntryRequest>(), Arg.Any<CancellationToken>());
    }


    [Fact]
    public async Task Update_ShouldReturnProblemDetails_WhenEditModeIsDifferent()
    {
        var entryId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        var entryRequest = new EntryRequest
        {
            Id = entryId,
            ExpiresIn = TimeSpan.FromDays(1),
            EditMode = EditMode.Advanced,
            TextContent = "Updated Text"
        };

        var existingEntryInfo = new EntryInfo(entryId, _creator.Id, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 
            EditMode.Simple, new Entry("Original content"), new List<ImageInfo>(), new EntryOpenGraphDescription("Original", "Original", null), 0);

        var entryInfoCacheKey = CacheKeyBuilder.BuildEntryInfoCacheKey(entryId);
        _dataStorageSubstitute.TryGet<EntryInfo?>(entryInfoCacheKey, cancellationToken)
            .Returns(new ValueTask<EntryInfo?>(existingEntryInfo));

        var localizedString = new LocalizedString("EntryEditModeMismatch", "Edit mode cannot be changed for existing entries.");
        _localizerSubstitute["EntryEditModeMismatch"]
            .Returns(localizedString);

        var result = await _entryInfoService.Update(_creator, entryRequest, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(localizedString.Value, result.Error.Detail);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.Error.Status);
        
        await _viewCountServiceSubstitute.DidNotReceive().Get(entryId, cancellationToken);
        await _entryServiceSubstitute.DidNotReceive().Add(Arg.Any<EntryRequest>(), Arg.Any<CancellationToken>());
    }


    private readonly IEntryInfoService _entryInfoService;
    private readonly Creator _creator;
    private readonly ILoggerFactory _loggerFactorySubstitute = Substitute.For<ILoggerFactory>();
    private readonly IOpenGraphService _openGraphServiceSubstitute = Substitute.For<IOpenGraphService>();
    private readonly IDataStorage _dataStorageSubstitute = Substitute.For<IDataStorage>();
    private readonly IReportService _reportServiceSubstitute = Substitute.For<IReportService>();
    private readonly IViewCountService _viewCountServiceSubstitute = Substitute.For<IViewCountService>();
    private readonly IImageService _imageServiceSubstitute = Substitute.For<IImageService>();
    private readonly IEntryService _entryServiceSubstitute = Substitute.For<IEntryService>();
    private readonly ICreatorService _creatorServiceSubstitute = Substitute.For<ICreatorService>();
    private readonly IStringLocalizer<ServerResources> _localizerSubstitute = Substitute.For<IStringLocalizer<ServerResources>>();
}