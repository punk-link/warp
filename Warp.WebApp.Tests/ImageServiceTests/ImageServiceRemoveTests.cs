using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using NSubstitute;
using System.Net;
using Warp.WebApp.Data;
using Warp.WebApp.Data.S3;
using Warp.WebApp.Helpers;
using Warp.WebApp.Services.Images;

namespace Warp.WebApp.Tests.ImageServiceTests;

public class ImageServiceRemoveTests
{
    public ImageServiceRemoveTests()
    {
        _localizerSubstitute = Substitute.For<IStringLocalizer<ServerResources>>();
        _dataStorageSubstitute = Substitute.For<IDataStorage>();
        _s3StorageSubstitute = Substitute.For<IS3FileStorage>();
        
        _imageService = new ImageService(_localizerSubstitute, _dataStorageSubstitute, _s3StorageSubstitute);
    }

    [Fact]
    public async Task Remove_WithoutHash_DeletesOnlyFile()
    {
        var entryId = Guid.NewGuid();
        var imageId = Guid.NewGuid();

        _dataStorageSubstitute.TryGet<string>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<string?>((string?)null));

        _s3StorageSubstitute.Delete(entryId.ToString(), imageId.ToString(), Arg.Any<CancellationToken>())
            .Returns(UnitResult.Success<ProblemDetails>());

        var result = await _imageService.Remove(entryId, imageId, CancellationToken.None);

        Assert.True(result.IsSuccess);

        // Verify that Remove was not called on the data storage
        await _dataStorageSubstitute.DidNotReceive()
            .Remove<string>(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }


    [Fact]
    public async Task Remove_WithHash_CleansUpHashAndDeletesFile()
    {
        var entryId = Guid.NewGuid();
        var imageId = Guid.NewGuid();

        _dataStorageSubstitute.TryGet<string>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<string?>("hash"));

        _s3StorageSubstitute.Delete(entryId.ToString(), imageId.ToString(), Arg.Any<CancellationToken>())
            .Returns(UnitResult.Success<ProblemDetails>());

        var result = await _imageService.Remove(entryId, imageId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        
        await _dataStorageSubstitute.Received(2)
            .Remove<string>(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }


    [Fact]
    public async Task Remove_FileDeletionFailed()
    {
        var entryId = Guid.NewGuid();
        var imageId = Guid.NewGuid();

        _dataStorageSubstitute.TryGet<string>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<string?>("hash"));

        _s3StorageSubstitute.Delete(entryId.ToString(), imageId.ToString(), Arg.Any<CancellationToken>())
            .Returns(UnitResult.Failure(ProblemDetailsHelper.Create("Error")));

        var result = await _imageService.Remove(entryId, imageId, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.Error.Status);
    }


    [Fact]
    public async Task Remove_Success()
    {
        var entryId = Guid.NewGuid();
        var imageId = Guid.NewGuid();

        _s3StorageSubstitute.Delete(entryId.ToString(), imageId.ToString(), Arg.Any<CancellationToken>())
            .Returns(UnitResult.Success<ProblemDetails>());

        var result = await _imageService.Remove(entryId, imageId, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }


    private readonly IS3FileStorage _s3StorageSubstitute;
    private readonly IStringLocalizer<ServerResources> _localizerSubstitute;
    private readonly IDataStorage _dataStorageSubstitute;
    private readonly ImageService _imageService;
}
