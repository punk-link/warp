using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Warp.WebApp.Constants.Logging;
using Warp.WebApp.Data;
using Warp.WebApp.Data.S3;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Models.Files;
using Warp.WebApp.Services.Images;

namespace Warp.WebApp.Tests.ImageServiceTests;

public class ImageServiceGetTests
{
    public ImageServiceGetTests()
    {
        _loggerFactorySubstitute = Substitute.For<ILoggerFactory>();
        _loggerSubstitute = Substitute.For<ILogger<ImageService>>();
        _loggerFactorySubstitute.CreateLogger<ImageService>().Returns(_loggerSubstitute);
        
        _dataStorageSubstitute = Substitute.For<IDataStorage>();
        _s3StorageSubstitute = Substitute.For<IS3FileStorage>();
        
        _imageService = new ImageService(_dataStorageSubstitute, _s3StorageSubstitute);
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

        using var stream = new MemoryStream([0x01, 0x02, 0x03]);
        var appFile = new AppFile(stream, "image/png");

        _s3StorageSubstitute
            .Get(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<AppFile, DomainError>(appFile));

        var result = await _imageService.Get(entryId, imageId, CancellationToken.None);

        Assert.Equal(result.Value.Content, appFile.Content);
        Assert.Equal(result.Value.Id, imageId);
        Assert.Equal(result.Value.ContentType, appFile.ContentMimeType);
    }
    

    private readonly IS3FileStorage _s3StorageSubstitute;
    private readonly ILoggerFactory _loggerFactorySubstitute;
    private readonly ILogger<ImageService> _loggerSubstitute;
    private readonly IDataStorage _dataStorageSubstitute;
    private readonly ImageService _imageService;
}
