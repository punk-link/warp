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

public class EntryInfoServiceCopyTests
{
    public EntryInfoServiceCopyTests()
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
    public async Task Copy_ShouldReturnNewEntryInfo_WhenSuccessful()
    {
        var originalEntryId = Guid.NewGuid();
        var newEntryId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var originalImageId = Guid.NewGuid();
        var imageInfos = new List<ImageInfo>
        {
            new(originalImageId, originalEntryId, new Uri("http://example.com/image.jpg"))
        };

        var originalEntryInfo = new EntryInfo(originalEntryId, _creator.Id, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(2), EditMode.Advanced,
            new Entry("Original content"), imageInfos, new EntryOpenGraphDescription("Test", "Test", imageInfos[0].Url), 10);

        _dataStorageSubstitute.TryGet<EntryInfo?>(Arg.Any<string>(), cancellationToken)
            .Returns(new ValueTask<EntryInfo?>(originalEntryInfo));

        var newImageId = Guid.NewGuid();
        var copiedImageInfos = new List<ImageInfo>
        {
            new(newImageId, newEntryId, new Uri("http://example.com/copied-image.jpg"))
        };

        _imageServiceSubstitute.Copy(originalEntryId, Arg.Any<Guid>(), imageInfos, cancellationToken)
            .Returns(Result.Success<List<ImageInfo>, ProblemDetails>(copiedImageInfos));

        var newEntry = new Entry("Original content");
        _entryServiceSubstitute.Add(Arg.Any<EntryRequest>(), cancellationToken)
            .Returns(Result.Success<Entry, ProblemDetails>(newEntry));

        _imageServiceSubstitute.GetAttached(Arg.Any<Guid>(), Arg.Is<List<Guid>>(list => list.Contains(newImageId)), cancellationToken)
            .Returns(Result.Success<List<ImageInfo>, ProblemDetails>(copiedImageInfos));

        _creatorServiceSubstitute.AttachEntry(Arg.Any<Creator>(), Arg.Any<EntryInfo>(), cancellationToken)
            .Returns(Result.Success<EntryInfo, ProblemDetails>(default));

        _dataStorageSubstitute.Set(Arg.Any<string>(), Arg.Any<EntryInfo>(), Arg.Any<TimeSpan>(), cancellationToken)
            .Returns(Result.Success());

        _openGraphServiceSubstitute.BuildDescription(Arg.Any<string>(), Arg.Any<Uri>())
            .Returns(new EntryOpenGraphDescription("Test", "Test", copiedImageInfos[0].Url));

        _localizerSubstitute["EntryExpirationPeriodEmptyErrorMessage"]
            .Returns(new LocalizedString("EntryExpirationPeriodEmptyErrorMessage", "Entry period is empty."));

        var result = await _entryInfoService.Copy(_creator, originalEntryId, cancellationToken);

        Assert.True(result.IsSuccess);
        await _imageServiceSubstitute.Received()
            .Copy(originalEntryId, Arg.Any<Guid>(), imageInfos, cancellationToken);

        await _entryServiceSubstitute.Received()
            .Add(Arg.Is<EntryRequest>(req => req.TextContent == originalEntryInfo.Entry.Content && req.EditMode == originalEntryInfo.EditMode),cancellationToken);
    }


    [Fact]
    public async Task Copy_ShouldReturnProblemDetails_WhenEntryNotFound()
    {
        var entryId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        _dataStorageSubstitute.TryGet<EntryInfo?>(Arg.Any<string>(), cancellationToken)
            .Returns(new ValueTask<EntryInfo?>((EntryInfo?)null));

        _localizerSubstitute["NotFoundErrorMessage"]
            .Returns(new LocalizedString("NotFoundErrorMessage", "Entry not found."));

        var result = await _entryInfoService.Copy(_creator, entryId, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal((int)HttpStatusCode.NotFound, result.Error.Status);
    }


    [Fact]
    public async Task Copy_ShouldReturnProblemDetails_WhenEntryDoesntBelongToCreator()
    {
        var entryId = Guid.NewGuid();
        var differentCreatorId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var entryInfo = new EntryInfo(entryId, differentCreatorId, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), EditMode.Simple,
            new Entry("Test content"), [], new EntryOpenGraphDescription("Test", "Test", null), 0);

        _dataStorageSubstitute.TryGet<EntryInfo?>(Arg.Any<string>(), cancellationToken)
            .Returns(new ValueTask<EntryInfo?>(entryInfo));

        var localizedString = new LocalizedString("NoPermissionErrorMessage", "Entry does not belong to creator.");
        _localizerSubstitute["NoPermissionErrorMessage"]
            .Returns(localizedString);

        var result = await _entryInfoService.Copy(_creator, entryId, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(localizedString.Value, result.Error.Detail);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.Error.Status);
    }


    [Fact]
    public async Task Copy_ShouldReturnProblemDetails_WhenImageCopyFails()
    {
        var originalEntryId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var imageInfos = new List<ImageInfo>
        {
            new(Guid.NewGuid(), originalEntryId, new Uri("http://example.com/image.jpg"))
        };

        var originalEntryInfo = new EntryInfo(originalEntryId, _creator.Id, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), EditMode.Advanced,
            new Entry("Original content"), imageInfos, new EntryOpenGraphDescription("Test", "Test", imageInfos[0].Url), 0);

        _dataStorageSubstitute.TryGet<EntryInfo?>(Arg.Any<string>(), cancellationToken)
            .Returns(new ValueTask<EntryInfo?>(originalEntryInfo));

        var problemDetails = ProblemDetailsHelper.Create("Failed to copy images");
        _imageServiceSubstitute.Copy(originalEntryId, Arg.Any<Guid>(), imageInfos, cancellationToken)
            .Returns(Result.Failure<List<ImageInfo>, ProblemDetails>(problemDetails));

        var result = await _entryInfoService.Copy(_creator, originalEntryId, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(problemDetails, result.Error);
    }


    [Fact]
    public async Task Copy_ShouldWorkWithNoImages_WhenOriginalEntryHasNoImages()
    {
        var originalEntryId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var originalEntryInfo = new EntryInfo(originalEntryId, _creator.Id, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), EditMode.Simple,
            new Entry("Original content"), [], new EntryOpenGraphDescription("Test", "Test", null), 0);

        _dataStorageSubstitute.TryGet<EntryInfo?>(Arg.Any<string>(), cancellationToken)
            .Returns(new ValueTask<EntryInfo?>(originalEntryInfo));

        var newEntry = new Entry("Original content");
        _entryServiceSubstitute.Add(Arg.Any<EntryRequest>(), cancellationToken)
            .Returns(Result.Success<Entry, ProblemDetails>(newEntry));

        _imageServiceSubstitute.GetAttached(Arg.Any<Guid>(), Arg.Any<List<Guid>>(), cancellationToken)
            .Returns(Result.Success<List<ImageInfo>, ProblemDetails>(new List<ImageInfo>()));

        _creatorServiceSubstitute.AttachEntry(Arg.Any<Creator>(), Arg.Any<EntryInfo>(), cancellationToken)
            .Returns(Result.Success<EntryInfo, ProblemDetails>(default));

        _dataStorageSubstitute.Set(Arg.Any<string>(), Arg.Any<EntryInfo>(), Arg.Any<TimeSpan>(), cancellationToken)
            .Returns(Result.Success());

        _openGraphServiceSubstitute.BuildDescription(Arg.Any<string>(), Arg.Any<Uri>())
            .Returns(new EntryOpenGraphDescription("Test", "Test", null));

        _localizerSubstitute["EntryExpirationPeriodEmptyErrorMessage"]
            .Returns(new LocalizedString("EntryExpirationPeriodEmptyErrorMessage", "Entry period is empty."));

        var result = await _entryInfoService.Copy(_creator, originalEntryId, cancellationToken);

        Assert.True(result.IsSuccess);
        await _imageServiceSubstitute.DidNotReceive()
            .Copy(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<List<ImageInfo>>(), Arg.Any<CancellationToken>());
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
