using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using Warp.WebApp.Data;
using Warp.WebApp.Helpers;
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

public class EntryInfoServiceGetTests
{
    public EntryInfoServiceGetTests()
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
    public async Task Get_ShouldIncrementViewCount_WhenEntryDoesNotBelongToCurrentCreator()
    {
        var entryId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        var viewCount = 42L;

        // Different creator than the one requesting
        var entryInfo = new EntryInfo(entryId, creatorId, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), EditMode.Simple,
            new Entry("Test content"), [], new EntryOpenGraphDescription("Test", "Test", null), viewCount: 5);

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

        // Same creator as the one requesting
        var entryInfo = new EntryInfo(entryId, _creator.Id, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), EditMode.Simple, 
            new Entry("Test content"), [], new EntryOpenGraphDescription("Test", "Test", null), viewCount: 5);

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
    public async Task Get_ShouldReturnProblemDetails_WhenEntryIsReported()
    {
        var entryId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        _reportServiceSubstitute.Contains(entryId, cancellationToken)
            .Returns(new ValueTask<bool>(true));

        _localizerSubstitute["NotFoundErrorMessage"]
            .Returns(new LocalizedString("NotFoundErrorMessage", "Entry not found."));

        var result = await _entryInfoService.Get(_creator, entryId, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal((int)HttpStatusCode.NotFound, result.Error.Status);
    }


    [Fact]
    public async Task Get_ShouldReturnProblemDetails_WhenEntryNotFound()
    {
        var entryId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        _reportServiceSubstitute.Contains(entryId, cancellationToken)
            .Returns(new ValueTask<bool>(false));

        _dataStorageSubstitute.TryGet<EntryInfo?>(Arg.Any<string>(), cancellationToken)
            .Returns(new ValueTask<EntryInfo?>((EntryInfo?)null));

        _localizerSubstitute["NotFoundErrorMessage"]
            .Returns(new LocalizedString("NotFoundErrorMessage", "Entry not found."));

        var result = await _entryInfoService.Get(_creator, entryId, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal((int)HttpStatusCode.NotFound, result.Error.Status);
    }


    [Fact]
    public async Task Get_ShouldReturnEntryWithCorrectData_WhenSuccessful()
    {
        var entryId = Guid.NewGuid();
        var viewCount = 10L;
        var cancellationToken = CancellationToken.None;

        var content = "Test content";
        var entryInfo = new EntryInfo(entryId, _creator.Id, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), EditMode.Advanced,
            new Entry(content), [], new EntryOpenGraphDescription("Test", "Test", null), 5);

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
    private readonly IOpenGraphService _openGraphServiceSubstitute = Substitute.For<IOpenGraphService>();
    private readonly IDataStorage _dataStorageSubstitute = Substitute.For<IDataStorage>();
    private readonly IReportService _reportServiceSubstitute = Substitute.For<IReportService>();
    private readonly IViewCountService _viewCountServiceSubstitute = Substitute.For<IViewCountService>();
    private readonly IImageService _imageServiceSubstitute = Substitute.For<IImageService>();
    private readonly IEntryService _entryServiceSubstitute = Substitute.For<IEntryService>();
    private readonly ICreatorService _creatorServiceSubstitute = Substitute.For<ICreatorService>();
    private readonly IStringLocalizer<ServerResources> _localizerSubstitute = Substitute.For<IStringLocalizer<ServerResources>>();
}
