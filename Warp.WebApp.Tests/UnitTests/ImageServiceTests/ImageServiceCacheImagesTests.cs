using CSharpFunctionalExtensions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Warp.WebApp.Data;
using Warp.WebApp.Data.S3;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Models.Files;
using Warp.WebApp.Models.Options;
using Warp.WebApp.Services.Images;

namespace Warp.WebApp.Tests.UnitTests.ImageServiceTests;

public class ImageServiceCacheImagesTests
{
    public ImageServiceCacheImagesTests()
    {
        _dataStorageSubstitute = Substitute.For<IDataStorage>();
        _s3StorageSubstitute = Substitute.For<IS3FileStorage>();
    }


    [Fact]
    public async Task CacheImages_S3GetFailed_SkipsImage()
    {
        var imageService = BuildImageService(maxCachableFileSize: 1_048_576);
        var entryId = Guid.NewGuid();
        var imageIds = new List<Guid> { Guid.NewGuid() };

        _s3StorageSubstitute.Get(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<AppFile, DomainError>(DomainErrors.S3GetObjectError()));

        await imageService.CacheImages(entryId, imageIds, TimeSpan.FromMinutes(10), CancellationToken.None);

        await _dataStorageSubstitute.DidNotReceive()
            .Set<CachedImage>(Arg.Any<string>(), Arg.Any<CachedImage>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());
    }


    [Fact]
    public async Task CacheImages_S3GetSuccess_StoresInCache()
    {
        var imageService = BuildImageService(maxCachableFileSize: 1_048_576);
        var entryId = Guid.NewGuid();
        var imageIds = new List<Guid> { Guid.NewGuid() };
        var expiresIn = TimeSpan.FromMinutes(10);

        using var stream = new MemoryStream([0x01, 0x02, 0x03]);
        var appFile = new AppFile(stream, "image/png");

        _s3StorageSubstitute.Get(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<AppFile, DomainError>(appFile));

        await imageService.CacheImages(entryId, imageIds, expiresIn, CancellationToken.None);

        await _dataStorageSubstitute.Received(1)
            .Set<CachedImage>(Arg.Any<string>(), Arg.Any<CachedImage>(), Arg.Is<TimeSpan>(ts => ts == expiresIn), Arg.Any<CancellationToken>());
    }


    [Fact]
    public async Task CacheImages_ImageExceedsMaxSize_SkipsCache()
    {
        var imageService = BuildImageService(maxCachableFileSize: 2);
        var entryId = Guid.NewGuid();
        var imageIds = new List<Guid> { Guid.NewGuid() };

        using var stream = new MemoryStream([0x01, 0x02, 0x03]);
        var appFile = new AppFile(stream, "image/png");

        _s3StorageSubstitute.Get(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<AppFile, DomainError>(appFile));

        await imageService.CacheImages(entryId, imageIds, TimeSpan.FromMinutes(10), CancellationToken.None);

        await _dataStorageSubstitute.DidNotReceive()
            .Set<CachedImage>(Arg.Any<string>(), Arg.Any<CachedImage>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());
    }


    [Fact]
    public async Task CacheImages_MultipleIds_ProcessesAll()
    {
        var imageService = BuildImageService(maxCachableFileSize: 1_048_576);
        var entryId = Guid.NewGuid();
        var imageIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var expiresIn = TimeSpan.FromMinutes(10);

        _s3StorageSubstitute.Get(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.Success<AppFile, DomainError>(new AppFile(new MemoryStream([0x01, 0x02, 0x03]), "image/png")));

        await imageService.CacheImages(entryId, imageIds, expiresIn, CancellationToken.None);

        await _dataStorageSubstitute.Received(imageIds.Count)
            .Set<CachedImage>(Arg.Any<string>(), Arg.Any<CachedImage>(), Arg.Is<TimeSpan>(ts => ts == expiresIn), Arg.Any<CancellationToken>());
    }


    private ImageService BuildImageService(long maxCachableFileSize)
        => new(_dataStorageSubstitute, _s3StorageSubstitute, Options.Create(new ImageCacheOptions { MaxCachableFileSize = maxCachableFileSize }));


    private readonly IS3FileStorage _s3StorageSubstitute;
    private readonly IDataStorage _dataStorageSubstitute;
}
