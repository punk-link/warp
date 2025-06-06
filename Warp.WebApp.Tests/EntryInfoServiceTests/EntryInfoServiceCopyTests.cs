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

public class EntryInfoServiceCopyTests
{
    public EntryInfoServiceCopyTests()
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

        var originalEntryInfo = new EntryInfo(
            id: originalEntryId, 
            creatorId: _creator.Id, 
            createdAt: DateTime.UtcNow.AddDays(-1), 
            expiresAt: DateTime.UtcNow.AddDays(2), 
            editMode: EditMode.Advanced,
            entry: new Entry("Original content"), 
            imageInfos: imageInfos, 
            openGraphDescription: new EntryOpenGraphDescription("Test", "Test", imageInfos[0].Url), 
            viewCount: 10);

        _dataStorageSubstitute.TryGet<EntryInfo?>(Arg.Any<string>(), cancellationToken)
            .Returns(new ValueTask<EntryInfo?>(originalEntryInfo));

        var newImageId = Guid.NewGuid();
        var copiedImageInfos = new List<ImageInfo>
        {
            new(id: newImageId, entryId: newEntryId, url: new Uri("http://example.com/copied-image.jpg"))
        };

        _imageServiceSubstitute.Copy(originalEntryId, Arg.Any<Guid>(), imageInfos, cancellationToken)
            .Returns(Result.Success<List<ImageInfo>, DomainError>(copiedImageInfos));

        var newEntry = new Entry("Original content");
        _entryServiceSubstitute.Add(Arg.Any<EntryRequest>(), cancellationToken)
            .Returns(Result.Success<Entry, DomainError>(newEntry));

        _imageServiceSubstitute.GetAttached(Arg.Any<Guid>(), Arg.Is<List<Guid>>(list => list.Contains(newImageId)), cancellationToken)
            .Returns(Result.Success<List<ImageInfo>, DomainError>(copiedImageInfos));

        _creatorServiceSubstitute.AttachEntry(Arg.Any<Creator>(), Arg.Any<EntryInfo>(), cancellationToken)
            .Returns(Result.Success<EntryInfo, DomainError>(default));

        _dataStorageSubstitute.Set(Arg.Any<string>(), Arg.Any<EntryInfo>(), Arg.Any<TimeSpan>(), cancellationToken)
            .Returns(UnitResult.Success<DomainError>());

        _openGraphServiceSubstitute.BuildDescription(Arg.Any<string>(), Arg.Any<Uri>())
            .Returns(new EntryOpenGraphDescription("Test", "Test", copiedImageInfos[0].Url));

        var result = await _entryInfoService.Copy(_creator, originalEntryId, cancellationToken);

        Assert.True(result.IsSuccess);
        await _imageServiceSubstitute.Received()
            .Copy(originalEntryId, Arg.Any<Guid>(), imageInfos, cancellationToken);

        await _entryServiceSubstitute.Received()
            .Add(Arg.Is<EntryRequest>(req => req.TextContent == originalEntryInfo.Entry.Content && req.EditMode == originalEntryInfo.EditMode), cancellationToken);
    }


    [Fact]
    public async Task Copy_ShouldReturnDomainError_WhenEntryNotFound()
    {
        var entryId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        _dataStorageSubstitute.TryGet<EntryInfo?>(Arg.Any<string>(), cancellationToken)
            .Returns(new ValueTask<EntryInfo?>((EntryInfo?)null));

        var result = await _entryInfoService.Copy(_creator, entryId, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(Constants.Logging.LogEvents.EntryNotFound, result.Error.Code);
    }


    [Fact]
    public async Task Copy_ShouldReturnDomainError_WhenEntryDoesntBelongToCreator()
    {
        var entryId = Guid.NewGuid();
        var differentCreatorId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var entryInfo = new EntryInfo(
            id: entryId, 
            creatorId: differentCreatorId, 
            createdAt: DateTime.UtcNow, 
            expiresAt: DateTime.UtcNow.AddDays(1), 
            editMode: EditMode.Simple,
            entry: new Entry("Test content"), 
            imageInfos: [], 
            openGraphDescription: new EntryOpenGraphDescription("Test", "Test", null), 
            viewCount: 0);

        _dataStorageSubstitute.TryGet<EntryInfo?>(Arg.Any<string>(), cancellationToken)
            .Returns(new ValueTask<EntryInfo?>(entryInfo));

        var result = await _entryInfoService.Copy(_creator, entryId, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(Constants.Logging.LogEvents.NoPermissionError, result.Error.Code);
    }


    [Fact]
    public async Task Copy_ShouldReturnDomainError_WhenImageCopyFails()
    {
        var originalEntryId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var imageInfos = new List<ImageInfo>
        {
            new(id: Guid.NewGuid(), entryId: originalEntryId, url: new Uri("http://example.com/image.jpg"))
        };

        var originalEntryInfo = new EntryInfo(
            id: originalEntryId, 
            creatorId: _creator.Id, 
            createdAt: DateTime.UtcNow, 
            expiresAt: DateTime.UtcNow.AddDays(1), 
            editMode: EditMode.Advanced,
            entry: new Entry("Original content"), 
            imageInfos: imageInfos, 
            openGraphDescription: new EntryOpenGraphDescription("Test", "Test", imageInfos[0].Url), 
            viewCount: 0);

        _dataStorageSubstitute.TryGet<EntryInfo?>(Arg.Any<string>(), cancellationToken)
            .Returns(new ValueTask<EntryInfo?>(originalEntryInfo));

        var domainError = DomainErrors.S3UploadObjectError();
        _imageServiceSubstitute.Copy(originalEntryId, Arg.Any<Guid>(), imageInfos, cancellationToken)
            .Returns(Result.Failure<List<ImageInfo>, DomainError>(domainError));

        var result = await _entryInfoService.Copy(_creator, originalEntryId, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(domainError.Code, result.Error.Code);
    }


    [Fact]
    public async Task Copy_ShouldWorkWithNoImages_WhenOriginalEntryHasNoImages()
    {
        var originalEntryId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var originalEntryInfo = new EntryInfo(
            id: originalEntryId, 
            creatorId: _creator.Id, 
            createdAt: DateTime.UtcNow, 
            expiresAt: DateTime.UtcNow.AddDays(1), 
            editMode: EditMode.Simple,
            entry: new Entry("Original content"), 
            imageInfos: [], 
            openGraphDescription: new EntryOpenGraphDescription("Test", "Test", null), 
            viewCount: 0);

        _dataStorageSubstitute.TryGet<EntryInfo?>(Arg.Any<string>(), cancellationToken)
            .Returns(new ValueTask<EntryInfo?>(originalEntryInfo));

        var newEntry = new Entry("Original content");
        _entryServiceSubstitute.Add(Arg.Any<EntryRequest>(), cancellationToken)
            .Returns(Result.Success<Entry, DomainError>(newEntry));

        _imageServiceSubstitute.GetAttached(Arg.Any<Guid>(), Arg.Any<List<Guid>>(), cancellationToken)
            .Returns(Result.Success<List<ImageInfo>, DomainError>(new List<ImageInfo>()));

        _creatorServiceSubstitute.AttachEntry(Arg.Any<Creator>(), Arg.Any<EntryInfo>(), cancellationToken)
            .Returns(Result.Success<EntryInfo, DomainError>(default));

        _dataStorageSubstitute.Set(Arg.Any<string>(), Arg.Any<EntryInfo>(), Arg.Any<TimeSpan>(), cancellationToken)
            .Returns(UnitResult.Success<DomainError>());

        _openGraphServiceSubstitute.BuildDescription(Arg.Any<string>(), Arg.Any<Uri>())
            .Returns(new EntryOpenGraphDescription("Test", "Test", null));

        var result = await _entryInfoService.Copy(_creator, originalEntryId, cancellationToken);

        Assert.True(result.IsSuccess);
        await _imageServiceSubstitute.DidNotReceive()
            .Copy(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<List<ImageInfo>>(), Arg.Any<CancellationToken>());
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
