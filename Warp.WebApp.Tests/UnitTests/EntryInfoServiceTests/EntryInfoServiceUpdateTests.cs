using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Warp.WebApp.Data;
using Warp.WebApp.Models.Creators;
using Warp.WebApp.Models.Entries;
using Warp.WebApp.Models.Entries.Enums;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Models.Images;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Creators;
using Warp.WebApp.Services.Entries;
using Warp.WebApp.Services.Images;
using Warp.WebApp.Services.OpenGraph;
using Warp.WebApp.Telemetry.Metrics;

namespace Warp.WebApp.Tests.UnitTests.EntryInfoServiceTests;

public class EntryInfoServiceUpdateTests
{
    public EntryInfoServiceUpdateTests()
    {
        _loggerFactorySubstitute.CreateLogger<EntryInfoService>().Returns(_loggerSubstitute);
        
        _entryInfoService = new EntryInfoService(
            _creatorServiceSubstitute,
            _dataStorageSubstitute,
            _entryServiceSubstitute,
            _imageServiceSubstitute,
            _entryImageLifecycleServiceSubstitute,
            _loggerFactorySubstitute,
            _openGraphServiceSubstitute,
            _reportServiceSubstitute,
            _viewCountServiceSubstitute,
            _entryInfoMetricsSubstitute
        );
        _creator = new Creator(Guid.NewGuid());

        _entryImageLifecycleServiceSubstitute.Track(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
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
            new(id: Guid.NewGuid(), entryId: entryId, url: new Uri("http://example.com/image.jpg"))
        };
        
        var existingEntryInfo = new EntryInfo(
            id: entryId, 
            creatorId: _creator.Id, 
            createdAt: DateTimeOffset.UtcNow, 
            expiresAt: DateTimeOffset.UtcNow.AddDays(1), 
            editMode: EditMode.Advanced, 
            entry: existingEntry, 
            imageInfos: new List<ImageInfo>(), 
            viewCount: 0);
        
        var updatedEntryInfo = new EntryInfo(
            id: entryId, 
            creatorId: _creator.Id, 
            createdAt: DateTimeOffset.UtcNow, 
            expiresAt: DateTimeOffset.UtcNow.AddDays(1), 
            editMode: EditMode.Advanced, 
            entry: updatedEntry, 
            imageInfos: imageInfos, 
            viewCount: 0);

        var entryInfoCacheKey = CacheKeyBuilder.BuildEntryInfoCacheKey(entryId);
        _dataStorageSubstitute.TryGet<EntryInfo?>(entryInfoCacheKey, cancellationToken)
            .Returns(new ValueTask<EntryInfo?>(existingEntryInfo));

        _viewCountServiceSubstitute.Get(entryId, cancellationToken)
            .Returns(Task.FromResult(0L));

        _entryServiceSubstitute.Add(Arg.Any<EntryRequest>(), cancellationToken)
            .Returns(Result.Success<Entry, DomainError>(updatedEntry));

        _imageServiceSubstitute.GetAttached(entryId, entryRequest.ImageIds, cancellationToken)
            .Returns(Result.Success<List<ImageInfo>, DomainError>(imageInfos));

        _creatorServiceSubstitute.AttachEntry(_creator, Arg.Any<EntryInfo>(), cancellationToken)
            .Returns(Result.Success<EntryInfo, DomainError>(updatedEntryInfo));

        _dataStorageSubstitute.Set(Arg.Any<string>(), Arg.Any<EntryInfo>(), Arg.Any<TimeSpan>(), cancellationToken)
            .Returns(UnitResult.Success<DomainError>());

        var result = await _entryInfoService.Update(_creator, entryRequest, cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(updatedEntry.Content, result.Value.Entry.Content);
        Assert.Equal(_creator.Id, result.Value.CreatorId);
        Assert.Equal(imageInfos.Count, result.Value.ImageInfos.Count);
        
        await _viewCountServiceSubstitute.Received(1).Get(entryId, cancellationToken);
    }


    [Fact]
    public async Task Update_ShouldReturnDomainError_WhenEntryDoesNotBelongToCreator()
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

        var existingEntryInfo = new EntryInfo(
            id: entryId, 
            creatorId: differentCreatorId, 
            createdAt: DateTimeOffset.UtcNow, 
            expiresAt: DateTimeOffset.UtcNow.AddDays(1), 
            editMode: EditMode.Simple, 
            entry: new Entry("Original content"), 
            imageInfos: new List<ImageInfo>(), 
            viewCount: 0);

        var entryInfoCacheKey = CacheKeyBuilder.BuildEntryInfoCacheKey(entryId);
        _dataStorageSubstitute.TryGet<EntryInfo?>(entryInfoCacheKey, cancellationToken)
            .Returns(new ValueTask<EntryInfo?>(existingEntryInfo));

        var result = await _entryInfoService.Update(_creator, entryRequest, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(Constants.Logging.LogEvents.NoPermissionError, result.Error.Code);
        
        await _entryServiceSubstitute.DidNotReceive().Add(Arg.Any<EntryRequest>(), Arg.Any<CancellationToken>());
    }


    [Fact]
    public async Task Update_ShouldReturnDomainError_WhenViewCountIsGreaterThanZero()
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

        var existingEntryInfo = new EntryInfo(
            id: entryId, 
            creatorId: _creator.Id, 
            createdAt: DateTimeOffset.UtcNow, 
            expiresAt: DateTimeOffset.UtcNow.AddDays(1), 
            editMode: EditMode.Simple, 
            entry: new Entry("Original content"), 
            imageInfos: new List<ImageInfo>(), 
            viewCount: 0);

        var entryInfoCacheKey = CacheKeyBuilder.BuildEntryInfoCacheKey(entryId);
        _dataStorageSubstitute.TryGet<EntryInfo?>(entryInfoCacheKey, cancellationToken)
            .Returns(new ValueTask<EntryInfo?>(existingEntryInfo));
            
        _viewCountServiceSubstitute.Get(entryId, cancellationToken)
            .Returns(Task.FromResult(5L));

        var result = await _entryInfoService.Update(_creator, entryRequest, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(Constants.Logging.LogEvents.EntryCannotBeEditedAfterViewed, result.Error.Code);
        
        await _viewCountServiceSubstitute.Received(1).Get(entryId, cancellationToken);
        await _entryServiceSubstitute.DidNotReceive().Add(Arg.Any<EntryRequest>(), Arg.Any<CancellationToken>());
    }


    [Fact]
    public async Task Update_ShouldReturnDomainError_WhenEntryDoesNotExist()
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

        var result = await _entryInfoService.Update(_creator, entryRequest, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(Constants.Logging.LogEvents.EntryNotFound, result.Error.Code);
        
        await _entryServiceSubstitute.DidNotReceive().Add(Arg.Any<EntryRequest>(), Arg.Any<CancellationToken>());
    }


    [Fact]
    public async Task Update_ShouldReturnDomainError_WhenEditModeIsDifferent()
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

        var existingEntryInfo = new EntryInfo(
            id: entryId, 
            creatorId: _creator.Id, 
            createdAt: DateTimeOffset.UtcNow, 
            expiresAt: DateTimeOffset.UtcNow.AddDays(1), 
            editMode: EditMode.Simple, 
            entry: new Entry("Original content"), 
            imageInfos: new List<ImageInfo>(), 
            viewCount: 0);

        var entryInfoCacheKey = CacheKeyBuilder.BuildEntryInfoCacheKey(entryId);
        _dataStorageSubstitute.TryGet<EntryInfo?>(entryInfoCacheKey, cancellationToken)
            .Returns(new ValueTask<EntryInfo?>(existingEntryInfo));

        var result = await _entryInfoService.Update(_creator, entryRequest, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(Constants.Logging.LogEvents.EntryEditModeMismatch, result.Error.Code);
        
        await _viewCountServiceSubstitute.DidNotReceive().Get(entryId, cancellationToken);
        await _entryServiceSubstitute.DidNotReceive().Add(Arg.Any<EntryRequest>(), Arg.Any<CancellationToken>());
    }


    private readonly IEntryInfoService _entryInfoService;
    private readonly Creator _creator;
    private readonly ILoggerFactory _loggerFactorySubstitute = Substitute.For<ILoggerFactory>();
    private readonly ILogger<EntryInfoService> _loggerSubstitute = Substitute.For<ILogger<EntryInfoService>>();
    private readonly IOpenGraphService _openGraphServiceSubstitute = Substitute.For<IOpenGraphService>();
    private readonly IDataStorage _dataStorageSubstitute = Substitute.For<IDataStorage>();
    private readonly IReportService _reportServiceSubstitute = Substitute.For<IReportService>();
    private readonly IViewCountService _viewCountServiceSubstitute = Substitute.For<IViewCountService>();
    private readonly IImageService _imageServiceSubstitute = Substitute.For<IImageService>();
    private readonly IEntryService _entryServiceSubstitute = Substitute.For<IEntryService>();
    private readonly ICreatorService _creatorServiceSubstitute = Substitute.For<ICreatorService>();
    private readonly IEntryImageLifecycleService _entryImageLifecycleServiceSubstitute = Substitute.For<IEntryImageLifecycleService>();
    private readonly IEntryInfoMetrics _entryInfoMetricsSubstitute = Substitute.For<IEntryInfoMetrics>();
}