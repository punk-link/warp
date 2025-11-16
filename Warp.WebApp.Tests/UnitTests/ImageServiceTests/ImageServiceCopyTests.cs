using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Warp.WebApp.Constants.Caching;
using Warp.WebApp.Constants.Logging;
using Warp.WebApp.Data;
using Warp.WebApp.Data.S3;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Models.Files;
using Warp.WebApp.Models.Images;
using Warp.WebApp.Services.Images;

namespace Warp.WebApp.Tests.UnitTests.ImageServiceTests;

public class ImageServiceCopyTests
{
    public ImageServiceCopyTests()
    {
        _loggerFactorySubstitute = Substitute.For<ILoggerFactory>();
        _loggerSubstitute = Substitute.For<ILogger<ImageService>>();
        _loggerFactorySubstitute.CreateLogger<ImageService>().Returns(_loggerSubstitute);
        
        _dataStorageSubstitute = Substitute.For<IDataStorage>();
        _s3StorageSubstitute = Substitute.For<IS3FileStorage>();
        
        _imageService = new ImageService(_dataStorageSubstitute, _s3StorageSubstitute);
    }


    [Fact]
    public async Task Copy_EmptySourceImages_ReturnsEmptyList()
    {
        var sourceEntryId = Guid.NewGuid();
        var targetEntryId = Guid.NewGuid();
        var sourceImages = new List<ImageInfo>();

        var result = await _imageService.Copy(sourceEntryId, targetEntryId, sourceImages, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }


    [Fact]
    public async Task Copy_GetSourceImageFailed_ReturnsFailure()
    {
        var sourceEntryId = Guid.NewGuid();
        var targetEntryId = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        var sourceImages = new List<ImageInfo>
        {
            new(id: imageId, entryId: sourceEntryId, url: new Uri($"https://example.com/{sourceEntryId}/{imageId}"))
        };

        _s3StorageSubstitute.Get(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<AppFile, DomainError>(DomainErrors.S3GetObjectError()));

        var result = await _imageService.Copy(sourceEntryId, targetEntryId, sourceImages, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(LogEvents.S3GetObjectError, result.Error.Code);
    }


    [Fact]
    public async Task Copy_SaveTargetImageFailed_ReturnsFailure()
    {
        var sourceEntryId = Guid.NewGuid();
        var targetEntryId = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        var sourceImages = new List<ImageInfo>
        {
            new(id: imageId, entryId: sourceEntryId, url: new Uri($"https://example.com/{sourceEntryId}/{imageId}"))
        };

        using var stream = new MemoryStream([0x01, 0x02, 0x03]);
        var appFile = new AppFile(stream, "image/png");

        _s3StorageSubstitute.Get(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<AppFile, DomainError>(appFile));

        _s3StorageSubstitute.Save(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<AppFile>(), Arg.Any<CancellationToken>())
            .Returns(UnitResult.Failure(DomainErrors.S3UploadObjectError()));

        var result = await _imageService.Copy(sourceEntryId, targetEntryId, sourceImages, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(LogEvents.S3UploadObjectError, result.Error.Code);
    }


    [Fact]
    public async Task Copy_Success_ReturnsCopiedImages()
    {
        var sourceEntryId = Guid.NewGuid();
        var targetEntryId = Guid.NewGuid();
        var imageId1 = Guid.NewGuid();
        var imageId2 = Guid.NewGuid();
        var sourceImages = new List<ImageInfo>
        {
            new(id: imageId1, entryId: sourceEntryId, url: new Uri($"https://example.com/{sourceEntryId}/{imageId1}")),
            new(id: imageId2, entryId: sourceEntryId, url: new Uri($"https://example.com/{sourceEntryId}/{imageId2}"))
        };

        using var stream1 = new MemoryStream([0x01, 0x02, 0x03]);
        var appFile1 = new AppFile(stream1, "image/png");
        
        using var stream2 = new MemoryStream([0x04, 0x05, 0x06]);
        var appFile2 = new AppFile(stream2, "image/jpeg");

        _s3StorageSubstitute.Get(Arg.Is<string>(s => s == sourceEntryId.ToString()), Arg.Is<string>(s => s == imageId1.ToString()), Arg.Any<CancellationToken>())
            .Returns(Result.Success<AppFile, DomainError>(appFile1));
        
        _s3StorageSubstitute.Get(Arg.Is<string>(s => s == sourceEntryId.ToString()), Arg.Is<string>(s => s == imageId2.ToString()), Arg.Any<CancellationToken>())
            .Returns(Result.Success<AppFile, DomainError>(appFile2));

        _s3StorageSubstitute.Save(Arg.Is<string>(s => s == targetEntryId.ToString()), Arg.Any<string>(), Arg.Any<AppFile>(), Arg.Any<CancellationToken>())
            .Returns(UnitResult.Success<DomainError>());

        _dataStorageSubstitute.Set(Arg.Any<string>(), Arg.Any<object>(), Arg.Is<TimeSpan>(ts => ts == CachingConstants.MaxSupportedCachingTime), Arg.Any<CancellationToken>())
            .Returns(UnitResult.Success<DomainError>());

        var result = await _imageService.Copy(sourceEntryId, targetEntryId, sourceImages, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(sourceImages.Count, result.Value.Count);
        
        foreach (var imageInfo in result.Value)
        {
            Assert.Equal(targetEntryId, imageInfo.EntryId);
            Assert.NotEqual(Guid.Empty, imageInfo.Id);
            Assert.NotNull(imageInfo.Url);
        }

        await _s3StorageSubstitute.Received(sourceImages.Count)
            .Get(Arg.Is<string>(s => s == sourceEntryId.ToString()), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _s3StorageSubstitute.Received(sourceImages.Count)
            .Save(Arg.Is<string>(s => s == targetEntryId.ToString()), Arg.Any<string>(), Arg.Any<AppFile>(), Arg.Any<CancellationToken>());
    }
    

    private readonly IS3FileStorage _s3StorageSubstitute;
    private readonly ILoggerFactory _loggerFactorySubstitute;
    private readonly ILogger<ImageService> _loggerSubstitute;
    private readonly IDataStorage _dataStorageSubstitute;
    private readonly ImageService _imageService;
}
