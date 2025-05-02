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
using Warp.WebApp.Models.Errors;
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
        _loggerFactorySubstitute.CreateLogger<EntryInfoService>().Returns(_loggerSubstitute);
        
        _entryInfoService = new EntryInfoService(
            _creatorServiceSubstitute,
            _dataStorageSubstitute,
            _entryServiceSubstitute,
            _imageServiceSubstitute,
            _loggerFactorySubstitute,
            _openGraphServiceSubstitute,
            _reportServiceSubstitute,
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
            new(id: imageId, entryId: entryId, url: new Uri("http://example.com/image.jpg")),
            new(id: Guid.NewGuid(), entryId: entryId, url: new Uri("http://example.com/image2.jpg"))
        };

        var entryInfo = new EntryInfo(
            id: entryId, 
            creatorId: _creator.Id, 
            createdAt: DateTime.UtcNow, 
            expiresAt: DateTime.UtcNow.AddDays(1), 
            editMode: EditMode.Advanced,
            entry: new Entry("Test content"), 
            imageInfos: imageInfos, 
            openGraphDescription: new EntryOpenGraphDescription("Test", "Test", imageInfos[0].Url), 
            viewCount: 0);

        _dataStorageSubstitute.TryGet<EntryInfo?>(Arg.Any<string>(), cancellationToken)
            .Returns(new ValueTask<EntryInfo?>(entryInfo));

        _dataStorageSubstitute.Set(Arg.Any<string>(), Arg.Any<EntryInfo>(), Arg.Any<TimeSpan>(), cancellationToken)
            .Returns(UnitResult.Success<DomainError>());

        _imageServiceSubstitute.Remove(entryId, imageId, cancellationToken)
            .Returns(UnitResult.Success<DomainError>());

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
            .Returns(UnitResult.Success<DomainError>());

        var result = await _entryInfoService.RemoveImage(_creator, entryId, imageId, cancellationToken);

        Assert.True(result.IsSuccess);
        await _dataStorageSubstitute.DidNotReceive().Set(Arg.Any<string>(), Arg.Any<EntryInfo>(), Arg.Any<TimeSpan>(), cancellationToken);
        await _imageServiceSubstitute.Received().Remove(entryId, imageId, cancellationToken);
    }


    [Fact]
    public async Task RemoveImage_ShouldReturnDomainError_WhenEntryDoesntBelongToCreator()
    {
        var entryId = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        var differentCreatorId = Guid.NewGuid();

        var imageInfos = new List<ImageInfo>
        {
            new(id: imageId, entryId: entryId, url: new Uri("http://example.com/image.jpg"))
        };

        var entryInfo = new EntryInfo(
            id: entryId, 
            creatorId: differentCreatorId, 
            createdAt: DateTime.UtcNow, 
            expiresAt: DateTime.UtcNow.AddDays(1), 
            editMode: EditMode.Advanced,
            entry: new Entry("Test content"), 
            imageInfos: imageInfos, 
            openGraphDescription: new EntryOpenGraphDescription("Test", "Test", imageInfos[0].Url), 
            viewCount: 0);

        _dataStorageSubstitute.TryGet<EntryInfo?>(Arg.Any<string>(), cancellationToken)
            .Returns(new ValueTask<EntryInfo?>(entryInfo));

        var result = await _entryInfoService.RemoveImage(_creator, entryId, imageId, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(Constants.Logging.LogEvents.NoPermissionError, result.Error.Code);
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
            new(id: imageId, entryId: entryId, url: new Uri("http://example.com/image.jpg"))
        };

        var entryInfo = new EntryInfo(
            id: entryId, 
            creatorId: _creator.Id, 
            createdAt: DateTime.UtcNow, 
            expiresAt: DateTime.UtcNow.AddDays(1), 
            editMode: EditMode.Advanced,
            entry: new Entry("Test content"), 
            imageInfos: imageInfos, 
            openGraphDescription: new EntryOpenGraphDescription("Test", "Test", imageInfos[0].Url), 
            viewCount: 0);

        _dataStorageSubstitute.TryGet<EntryInfo?>(Arg.Any<string>(), cancellationToken)
            .Returns(new ValueTask<EntryInfo?>(entryInfo));

        _dataStorageSubstitute.Set(Arg.Any<string>(), Arg.Any<EntryInfo>(), Arg.Any<TimeSpan>(), cancellationToken)
            .Returns(UnitResult.Success<DomainError>());

        var domainError = DomainErrors.S3DeleteObjectError();
        _imageServiceSubstitute.Remove(entryId, imageId, cancellationToken)
            .Returns(UnitResult.Failure<DomainError>(domainError));

        var result = await _entryInfoService.RemoveImage(_creator, entryId, imageId, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(domainError.Code, result.Error.Code);
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
}
