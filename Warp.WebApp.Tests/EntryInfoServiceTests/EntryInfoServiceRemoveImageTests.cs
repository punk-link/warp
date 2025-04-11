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

public class EntryInfoServiceRemoveImageTests
{
    public EntryInfoServiceRemoveImageTests()
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
    public async Task RemoveImage_ShouldReturnSuccess_WhenImageIsAttachedAndBelongsToCreator()
    {
        var entryId = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var imageInfos = new List<ImageInfo>
        {
            new(imageId, entryId, new Uri("http://example.com/image.jpg")),
            new(Guid.NewGuid(), entryId, new Uri("http://example.com/image2.jpg"))
        };

        var entryInfo = new EntryInfo(entryId, _creator.Id, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), EditMode.Advanced,
            new Entry("Test content"), imageInfos, new EntryOpenGraphDescription("Test", "Test", imageInfos[0].Url), 0);

        _dataStorageSubstitute.TryGet<EntryInfo?>(Arg.Any<string>(), cancellationToken)
            .Returns(new ValueTask<EntryInfo?>(entryInfo));

        _dataStorageSubstitute.Set(Arg.Any<string>(), Arg.Any<EntryInfo>(), Arg.Any<TimeSpan>(), cancellationToken)
            .Returns(Result.Success());

        _imageServiceSubstitute.Remove(entryId, imageId, cancellationToken)
            .Returns(UnitResult.Success<ProblemDetails>());

        var result = await _entryInfoService.RemoveImage(_creator, entryId, imageId, cancellationToken);

        Assert.True(result.IsSuccess);
        await _dataStorageSubstitute.Received().Set(Arg.Any<string>(),
            Arg.Is<EntryInfo>(info => info.ImageInfos.Count == 1 && !info.ImageInfos.Any(img => img.Id == imageId)),
            Arg.Any<TimeSpan>(), cancellationToken);
        await _imageServiceSubstitute.Received().Remove(entryId, imageId, cancellationToken);
    }


    [Fact]
    public async Task RemoveImage_ShouldRemoveUnattachedImage_WhenEntryInfoNotFound()
    {
        var entryId = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        _dataStorageSubstitute.TryGet<EntryInfo?>(Arg.Any<string>(), cancellationToken)
            .Returns(new ValueTask<EntryInfo?>((EntryInfo?)null));

        _imageServiceSubstitute.Remove(entryId, imageId, cancellationToken)
            .Returns(UnitResult.Success<ProblemDetails>());

        var result = await _entryInfoService.RemoveImage(_creator, entryId, imageId, cancellationToken);

        Assert.True(result.IsSuccess);
        await _dataStorageSubstitute.DidNotReceive().Set(Arg.Any<string>(), Arg.Any<EntryInfo>(), Arg.Any<TimeSpan>(), cancellationToken);
        await _imageServiceSubstitute.Received().Remove(entryId, imageId, cancellationToken);
    }


    [Fact]
    public async Task RemoveImage_ShouldReturnProblemDetails_WhenEntryDoesntBelongToCreator()
    {
        var entryId = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        var differentCreatorId = Guid.NewGuid();

        var imageInfos = new List<ImageInfo>
        {
            new(imageId, entryId, new Uri("http://example.com/image.jpg"))
        };

        var entryInfo = new EntryInfo(entryId, differentCreatorId, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), EditMode.Advanced,
            new Entry("Test content"), imageInfos, new EntryOpenGraphDescription("Test", "Test", imageInfos[0].Url), 0);

        _dataStorageSubstitute.TryGet<EntryInfo?>(Arg.Any<string>(), cancellationToken)
            .Returns(new ValueTask<EntryInfo?>(entryInfo));

        var localizedString = new LocalizedString("NoPermissionErrorMessage", "Entry does not belong to creator.");
        _localizerSubstitute["NoPermissionErrorMessage"]
            .Returns(localizedString);

        var result = await _entryInfoService.RemoveImage(_creator, entryId, imageId, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(localizedString.Value, result.Error.Detail);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.Error.Status);
        await _imageServiceSubstitute.DidNotReceive().Remove(entryId, imageId, cancellationToken);
    }


    [Fact]
    public async Task RemoveImage_ShouldReturnFailure_WhenImageServiceRemoveFails()
    {
        var entryId = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var imageInfos = new List<ImageInfo>
        {
            new(imageId, entryId, new Uri("http://example.com/image.jpg"))
        };

        var entryInfo = new EntryInfo(entryId, _creator.Id, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), EditMode.Advanced,
            new Entry("Test content"), imageInfos, new EntryOpenGraphDescription("Test", "Test", imageInfos[0].Url), 0);

        _dataStorageSubstitute.TryGet<EntryInfo?>(Arg.Any<string>(), cancellationToken)
            .Returns(new ValueTask<EntryInfo?>(entryInfo));

        _dataStorageSubstitute.Set(Arg.Any<string>(), Arg.Any<EntryInfo>(), Arg.Any<TimeSpan>(), cancellationToken)
            .Returns(Result.Success());

        var problemDetails = ProblemDetailsHelper.Create("Failed to remove image");
        _imageServiceSubstitute.Remove(entryId, imageId, cancellationToken)
            .Returns(UnitResult.Failure<ProblemDetails>(problemDetails));

        var result = await _entryInfoService.RemoveImage(_creator, entryId, imageId, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(problemDetails, result.Error);
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
