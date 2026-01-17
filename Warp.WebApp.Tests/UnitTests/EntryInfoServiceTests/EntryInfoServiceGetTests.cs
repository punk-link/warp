using Microsoft.Extensions.Logging;
using NSubstitute;
using Warp.WebApp.Data;
using Warp.WebApp.Models.Creators;
using Warp.WebApp.Models.Entries;
using Warp.WebApp.Models.Entries.Enums;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Creators;
using Warp.WebApp.Services.Entries;
using Warp.WebApp.Services.Images;
using Warp.WebApp.Services.OpenGraph;
using Warp.WebApp.Telemetry.Metrics;

namespace Warp.WebApp.Tests.UnitTests.EntryInfoServiceTests;

public class EntryInfoServiceGetTests
{
    public EntryInfoServiceGetTests()
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
    public async Task Get_ShouldIncrementViewCount_WhenEntryDoesNotBelongToCurrentCreator()
    {
        var entryId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        var viewCount = 42L;

        var entryInfo = new EntryInfo(
            id: entryId, 
            creatorId: creatorId, 
            createdAt: DateTimeOffset.UtcNow, 
            expiresAt: DateTimeOffset.UtcNow.AddDays(1), 
            editMode: EditMode.Simple,
            entry: new Entry("Test content"), 
            imageInfos: [], 
            viewCount: 5);

        _dataStorageSubstitute.TryGet<EntryInfo?>(Arg.Any<string>(), cancellationToken)
            .Returns(new ValueTask<EntryInfo?>(entryInfo));

        _reportServiceSubstitute.Contains(entryId, cancellationToken)
            .Returns(new ValueTask<bool>(false));

        _viewCountServiceSubstitute.AddAndGet(entryId, cancellationToken)
            .Returns(Task.FromResult(viewCount));

        var result = await _entryInfoService.Get(_creator, entryId, cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(viewCount, result.Value.ViewCount);
        await _viewCountServiceSubstitute.Received().AddAndGet(entryId, cancellationToken);
        await _viewCountServiceSubstitute.DidNotReceive().Get(entryId, cancellationToken);
    }


    [Fact]
    public async Task Get_ShouldNotIncrementViewCount_WhenEntryBelongsToCurrentCreator()
    {
        var entryId = Guid.NewGuid();
        var viewCount = 42L;
        var cancellationToken = CancellationToken.None;

        var entryInfo = new EntryInfo(
            id: entryId, 
            creatorId: _creator.Id, 
            createdAt: DateTimeOffset.UtcNow, 
            expiresAt: DateTimeOffset.UtcNow.AddDays(1), 
            editMode: EditMode.Simple, 
            entry: new Entry("Test content"), 
            imageInfos: [], 
            viewCount: 5);

        _dataStorageSubstitute.TryGet<EntryInfo?>(Arg.Any<string>(), cancellationToken)
            .Returns(new ValueTask<EntryInfo?>(entryInfo));

        _reportServiceSubstitute.Contains(entryId, cancellationToken)
            .Returns(new ValueTask<bool>(false));

        _viewCountServiceSubstitute.Get(entryId, cancellationToken)
            .Returns(Task.FromResult(viewCount));

        var result = await _entryInfoService.Get(_creator, entryId, cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(viewCount, result.Value.ViewCount);
        await _viewCountServiceSubstitute.Received().Get(entryId, cancellationToken);
        await _viewCountServiceSubstitute.DidNotReceive().AddAndGet(entryId, cancellationToken);
    }


    [Fact]
    public async Task Get_ShouldReturnDomainError_WhenEntryIsReported()
    {
        var entryId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        _reportServiceSubstitute.Contains(entryId, cancellationToken)
            .Returns(new ValueTask<bool>(true));

        var result = await _entryInfoService.Get(_creator, entryId, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(Constants.Logging.LogEvents.EntryNotFound, result.Error.Code);
    }


    [Fact]
    public async Task Get_ShouldReturnDomainError_WhenEntryNotFound()
    {
        var entryId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        _reportServiceSubstitute.Contains(entryId, cancellationToken)
            .Returns(new ValueTask<bool>(false));

        _dataStorageSubstitute.TryGet<EntryInfo?>(Arg.Any<string>(), cancellationToken)
            .Returns(new ValueTask<EntryInfo?>((EntryInfo?)null));

        var result = await _entryInfoService.Get(_creator, entryId, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(Constants.Logging.LogEvents.EntryNotFound, result.Error.Code);
    }


    [Fact]
    public async Task Get_ShouldReturnEntryWithCorrectData_WhenSuccessful()
    {
        var entryId = Guid.NewGuid();
        var viewCount = 10L;
        var cancellationToken = CancellationToken.None;

        var content = "Test content";
        var entryInfo = new EntryInfo(
            id: entryId, 
            creatorId: _creator.Id, 
            createdAt: DateTimeOffset.UtcNow, 
            expiresAt: DateTimeOffset.UtcNow.AddDays(1), 
            editMode: EditMode.Advanced,
            entry: new Entry(content), 
            imageInfos: [], 
            viewCount: 5);

        _dataStorageSubstitute.TryGet<EntryInfo?>(Arg.Any<string>(), cancellationToken)
            .Returns(new ValueTask<EntryInfo?>(entryInfo));

        _reportServiceSubstitute.Contains(entryId, cancellationToken)
            .Returns(new ValueTask<bool>(false));

        _viewCountServiceSubstitute.Get(entryId, cancellationToken)
            .Returns(Task.FromResult(viewCount));

        var result = await _entryInfoService.Get(_creator, entryId, cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(entryId, result.Value.Id);
        Assert.Equal(content, result.Value.Entry.Content);
        Assert.Equal(_creator.Id, result.Value.CreatorId);
        Assert.Equal(viewCount, result.Value.ViewCount);
        Assert.Equal(entryInfo.ExpiresAt, result.Value.ExpiresAt);
        Assert.Equal(entryInfo.CreatedAt, result.Value.CreatedAt);
        Assert.Equal(entryInfo.EditMode, result.Value.EditMode);
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
