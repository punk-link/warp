using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using NSubstitute;
using System.Net;
using Warp.WebApp.Constants.Caching;
using Warp.WebApp.Data;
using Warp.WebApp.Data.S3;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Files;
using Warp.WebApp.Services.Images;

namespace Warp.WebApp.Tests.ImageServiceTests;

public class ImageServiceCopyTests
{
    public ImageServiceCopyTests()
    {
        _localizerSubstitute = Substitute.For<IStringLocalizer<ServerResources>>();
        _dataStorageSubstitute = Substitute.For<IDataStorage>();
        _s3StorageSubstitute = Substitute.For<IS3FileStorage>();
        
        _imageService = new ImageService(_localizerSubstitute, _dataStorageSubstitute, _s3StorageSubstitute);
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
            new(imageId, sourceEntryId, new Uri($"https://example.com/{sourceEntryId}/{imageId}"))
        };

        _s3StorageSubstitute.Get(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<AppFile, ProblemDetails>(ProblemDetailsHelper.Create("Failed to get image")));

        var result = await _imageService.Copy(sourceEntryId, targetEntryId, sourceImages, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.Error.Status);
    }


    [Fact]
    public async Task Copy_SaveTargetImageFailed_ReturnsFailure()
    {
        var sourceEntryId = Guid.NewGuid();
        var targetEntryId = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        var sourceImages = new List<ImageInfo>
        {
            new(imageId, sourceEntryId, new Uri($"https://example.com/{sourceEntryId}/{imageId}"))
        };

        using var stream = new MemoryStream([0x01, 0x02, 0x03]);
        var appFile = new AppFile(stream, "image/png");

        _s3StorageSubstitute.Get(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<AppFile, ProblemDetails>(appFile));

        _s3StorageSubstitute.Save(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<AppFile>(), Arg.Any<CancellationToken>())
            .Returns(UnitResult.Failure(ProblemDetailsHelper.Create("Failed to save image")));

        var result = await _imageService.Copy(sourceEntryId, targetEntryId, sourceImages, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.Error.Status);
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
            new(imageId1, sourceEntryId, new Uri($"https://example.com/{sourceEntryId}/{imageId1}")),
            new(imageId2, sourceEntryId, new Uri($"https://example.com/{sourceEntryId}/{imageId2}"))
        };

        using var stream1 = new MemoryStream([0x01, 0x02, 0x03]);
        var appFile1 = new AppFile(stream1, "image/png");
        
        using var stream2 = new MemoryStream([0x04, 0x05, 0x06]);
        var appFile2 = new AppFile(stream2, "image/jpeg");

        _s3StorageSubstitute.Get(Arg.Is<string>(s => s == sourceEntryId.ToString()), Arg.Is<string>(s => s == imageId1.ToString()), Arg.Any<CancellationToken>())
            .Returns(Result.Success<AppFile, ProblemDetails>(appFile1));
        
        _s3StorageSubstitute.Get(Arg.Is<string>(s => s == sourceEntryId.ToString()), Arg.Is<string>(s => s == imageId2.ToString()), Arg.Any<CancellationToken>())
            .Returns(Result.Success<AppFile, ProblemDetails>(appFile2));

        _s3StorageSubstitute.Save(Arg.Is<string>(s => s == targetEntryId.ToString()), Arg.Any<string>(), Arg.Any<AppFile>(), Arg.Any<CancellationToken>())
            .Returns(UnitResult.Success<ProblemDetails>());

        _dataStorageSubstitute.Set(Arg.Any<string>(), Arg.Any<object>(), Arg.Is<TimeSpan>(ts => ts == CachingConstants.MaxSupportedCachingTime), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

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
    private readonly IStringLocalizer<ServerResources> _localizerSubstitute;
    private readonly IDataStorage _dataStorageSubstitute;
    private readonly ImageService _imageService;
}
