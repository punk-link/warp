using CSharpFunctionalExtensions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Warp.WebApp.Constants.Logging;
using Warp.WebApp.Data;
using Warp.WebApp.Data.S3;
using Warp.WebApp.Models.Entries;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Models.Files;
using Warp.WebApp.Models.Options;
using Warp.WebApp.Services.Images;

namespace Warp.WebApp.Tests.UnitTests.ImageServiceTests;

public class ImageServiceGetTests
{
    public ImageServiceGetTests()
    {
        _dataStorageSubstitute = Substitute.For<IDataStorage>();
        _s3StorageSubstitute = Substitute.For<IS3FileStorage>();

        _imageService = new ImageService(_dataStorageSubstitute, _s3StorageSubstitute, Options.Create(new ImageCacheOptions { MaxCachableFileSize = 1_048_576 }));
    }


    [Fact]
    public async Task Get_ImageFoundInCache_ReturnsCachedImage()
    {
        var entryId = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        var imageBytes = new byte[] { 0x01, 0x02, 0x03 };

        _dataStorageSubstitute.TryGet<CachedImage>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_ => new ValueTask<CachedImage>(new CachedImage { Content = imageBytes, ContentType = "image/png" }));

        var result = await _imageService.Get(entryId, imageId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(imageId, result.Value.Id);
        Assert.Equal("image/png", result.Value.ContentType);
        Assert.Equal(imageBytes, ((MemoryStream)result.Value.Content).ToArray());

        await _s3StorageSubstitute.DidNotReceive()
            .Get(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }


    [Fact]
    public async Task Get_GetFromStorage_Failed()
    {
        var entryId = Guid.NewGuid();
        var imageId = Guid.NewGuid();

        _s3StorageSubstitute.Get(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<AppFile, DomainError>(DomainErrors.S3GetObjectError()));

        var result = await _imageService.Get(entryId, imageId, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(LogEvents.S3GetObjectError, result.Error.Code);
    }


    [Fact]
    public async Task Get_GetFromStorage_Success()
    {
        var entryId = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        var imageBytes = new byte[] { 0x01, 0x02, 0x03 };

        _dataStorageSubstitute.TryGet<EntryInfo?>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<EntryInfo?>((EntryInfo?)null));

        using var stream = new MemoryStream(imageBytes);
        var appFile = new AppFile(stream, "image/png");

        _s3StorageSubstitute
            .Get(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<AppFile, DomainError>(appFile));

        var result = await _imageService.Get(entryId, imageId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(imageId, result.Value.Id);
        Assert.Equal("image/png", result.Value.ContentType);
        Assert.Equal(imageBytes, ((MemoryStream)result.Value.Content).ToArray());
    }
    

    private readonly IS3FileStorage _s3StorageSubstitute;
    private readonly IDataStorage _dataStorageSubstitute;
    private readonly ImageService _imageService;
}
