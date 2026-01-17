using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Warp.WebApp.Constants.Logging;
using Warp.WebApp.Data;
using Warp.WebApp.Data.S3;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Services.Images;

namespace Warp.WebApp.Tests.UnitTests.ImageServiceTests;

public class ImageServiceRemoveTests
{
    public ImageServiceRemoveTests()
    {
        _loggerFactorySubstitute = Substitute.For<ILoggerFactory>();
        _loggerSubstitute = Substitute.For<ILogger<ImageService>>();
        _loggerFactorySubstitute.CreateLogger<ImageService>().Returns(_loggerSubstitute);
        
        _dataStorageSubstitute = Substitute.For<IDataStorage>();
        _s3StorageSubstitute = Substitute.For<IS3FileStorage>();
        
        _imageService = new ImageService(_dataStorageSubstitute, _s3StorageSubstitute);
    }


    [Fact]
    public async Task Remove_NoHash_DeletesFromS3()
    {
        var entryId = Guid.NewGuid();
        var imageId = Guid.NewGuid();

        _dataStorageSubstitute.TryGet<string>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((string?)null);

        _s3StorageSubstitute.Delete(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(UnitResult.Success<DomainError>());

        var result = await _imageService.Remove(entryId, imageId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        
        await _dataStorageSubstitute.DidNotReceive()
            .Remove<string>(Arg.Any<string>(), Arg.Any<CancellationToken>());
        
        await _s3StorageSubstitute.Received(1)
            .Delete(Arg.Is<string>(s => s == entryId.ToString()), Arg.Is<string>(s => s == imageId.ToString()), Arg.Any<CancellationToken>());
    }


    [Fact]
    public async Task Remove_WithHash_RemovesHashAndDeletesFromS3()
    {
        var entryId = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        var hash = "some-hash";

        _dataStorageSubstitute.TryGet<string>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(hash);

        _s3StorageSubstitute.Delete(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(UnitResult.Success<DomainError>());

        var result = await _imageService.Remove(entryId, imageId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        
        await _dataStorageSubstitute.Received(2)
            .Remove<string>(Arg.Any<string>(), Arg.Any<CancellationToken>());
        
        await _s3StorageSubstitute.Received(1)
            .Delete(Arg.Is<string>(s => s == entryId.ToString()), Arg.Is<string>(s => s == imageId.ToString()), Arg.Any<CancellationToken>());
    }


    [Fact]
    public async Task Remove_DeleteFailed_ReturnsFailure()
    {
        var entryId = Guid.NewGuid();
        var imageId = Guid.NewGuid();

        _dataStorageSubstitute.TryGet<string>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((string?)null);

        _s3StorageSubstitute.Delete(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(UnitResult.Failure(DomainErrors.S3DeleteObjectError()));

        var result = await _imageService.Remove(entryId, imageId, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(LogEvents.S3DeleteObjectError, result.Error.Code);
    }


    [Fact]
    public async Task Remove_Success()
    {
        var entryId = Guid.NewGuid();
        var imageId = Guid.NewGuid();

        _s3StorageSubstitute.Delete(entryId.ToString(), imageId.ToString(), Arg.Any<CancellationToken>())
            .Returns(UnitResult.Success<DomainError>());

        var result = await _imageService.Remove(entryId, imageId, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }
    

    private readonly IS3FileStorage _s3StorageSubstitute;
    private readonly ILoggerFactory _loggerFactorySubstitute;
    private readonly ILogger<ImageService> _loggerSubstitute;
    private readonly IDataStorage _dataStorageSubstitute;
    private readonly ImageService _imageService;
}
