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

public class ImageServiceGetAttachedTests
{
    public ImageServiceGetAttachedTests()
    {
        _localizerSubstitute = Substitute.For<IStringLocalizer<ServerResources>>();
        _dataStorageSubstitute = Substitute.For<IDataStorage>();
        _s3StorageSubstitute = Substitute.For<IS3FileStorage>();
        
        _imageService = new ImageService(_localizerSubstitute, _dataStorageSubstitute, _s3StorageSubstitute);
    }

    [Fact]
    public async Task GetAttached_ContainsFailed()
    {
        var entryId = Guid.NewGuid();
        var imageIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        _s3StorageSubstitute.Contains(Arg.Any<string>(), Arg.Any<List<string>>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<HashSet<string>, ProblemDetails>(ProblemDetailsHelper.Create("Error")));

        var result = await _imageService.GetAttached(entryId, imageIds, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.Error.Status);
    }


    [Fact]
    public async Task GetAttached_ContainsSuccess()
    {
        var entryId = Guid.NewGuid();
        var imageIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        _s3StorageSubstitute.Contains(Arg.Any<string>(), Arg.Any<List<string>>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<HashSet<string>, ProblemDetails>(new HashSet<string>()));

        var result = await _imageService.GetAttached(entryId, imageIds, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    
    private readonly IS3FileStorage _s3StorageSubstitute;
    private readonly IStringLocalizer<ServerResources> _localizerSubstitute;
    private readonly IDataStorage _dataStorageSubstitute;
    private readonly ImageService _imageService;
}
