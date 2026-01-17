using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Warp.WebApp.Constants.Logging;
using Warp.WebApp.Data;
using Warp.WebApp.Data.S3;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Services.Images;

namespace Warp.WebApp.Tests.UnitTests.ImageServiceTests;

public class ImageServiceGetAttachedTests
{
    public ImageServiceGetAttachedTests()
    {
        _loggerFactorySubstitute = Substitute.For<ILoggerFactory>();
        _loggerSubstitute = Substitute.For<ILogger<ImageService>>();
        _loggerFactorySubstitute.CreateLogger<ImageService>().Returns(_loggerSubstitute);
        
        _dataStorageSubstitute = Substitute.For<IDataStorage>();
        _s3StorageSubstitute = Substitute.For<IS3FileStorage>();
        
        _imageService = new ImageService(_dataStorageSubstitute, _s3StorageSubstitute);
    }


    [Fact]
    public async Task GetAttached_ContainsFailed()
    {
        var entryId = Guid.NewGuid();
        var imageIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        _s3StorageSubstitute.Contains(Arg.Any<string>(), Arg.Any<List<string>>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<HashSet<string>, DomainError>(DomainErrors.S3ListObjectsError()));

        var result = await _imageService.GetAttached(entryId, imageIds, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(LogEvents.S3ListObjectsError, result.Error.Code);
    }


    [Fact]
    public async Task GetAttached_ContainsSuccess()
    {
        var entryId = Guid.NewGuid();
        var imageIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        _s3StorageSubstitute.Contains(Arg.Any<string>(), Arg.Any<List<string>>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<HashSet<string>, DomainError>(new HashSet<string>()));

        var result = await _imageService.GetAttached(entryId, imageIds, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    
    private readonly IS3FileStorage _s3StorageSubstitute;
    private readonly ILoggerFactory _loggerFactorySubstitute;
    private readonly ILogger<ImageService> _loggerSubstitute;
    private readonly IDataStorage _dataStorageSubstitute;
    private readonly ImageService _imageService;
}
