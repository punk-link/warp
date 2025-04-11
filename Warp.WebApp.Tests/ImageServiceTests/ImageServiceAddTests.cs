using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using NSubstitute;
using System.Net;
using Warp.WebApp.Constants.Caching;
using Warp.WebApp.Data;
using Warp.WebApp.Data.S3;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models.Files;
using Warp.WebApp.Services.Images;

namespace Warp.WebApp.Tests.ImageServiceTests;

public class ImageServiceAddTests
{
    public ImageServiceAddTests()
    {
        _localizerSubstitute = Substitute.For<IStringLocalizer<ServerResources>>();
        _dataStorageSubstitute = Substitute.For<IDataStorage>();
        _s3StorageSubstitute = Substitute.For<IS3FileStorage>();
        
        _imageService = new ImageService(_localizerSubstitute, _dataStorageSubstitute, _s3StorageSubstitute);
    }


    [Fact]
    public async Task Add_DuplicateImage_ReturnsFailure()
    {
        var entryId = Guid.NewGuid();
        using var stream = new MemoryStream([0x01, 0x02, 0x03]);
        var appFile = new AppFile(stream, "image/png");
        
        _dataStorageSubstitute.Contains<bool>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<bool>(true));

        var result = await _imageService.Add(entryId, appFile, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.Error.Status);
    }


    [Fact]
    public async Task Add_NewImage_SavesHashInDataStorage()
    {
        var entryId = Guid.NewGuid();
        using var stream = new MemoryStream([0x01, 0x02, 0x03]);
        var appFile = new AppFile(stream, "image/png");
        
        _dataStorageSubstitute.Contains<bool>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<bool>(false));

        _dataStorageSubstitute.Set(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        _s3StorageSubstitute.Save(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<AppFile>(), Arg.Any<CancellationToken>())
            .Returns(UnitResult.Success<ProblemDetails>());

        var result = await _imageService.Add(entryId, appFile, CancellationToken.None);

        Assert.True(result.IsSuccess);
        
        await _dataStorageSubstitute.Received(2)
            .Set(Arg.Any<string>(), Arg.Any<object>(), Arg.Is<TimeSpan>(ts => ts == CachingConstants.MaxSupportedCachingTime), Arg.Any<CancellationToken>());
    }
    

    [Fact]
    public async Task Add_SaveToStorage_Failed()
    {
        var entryId = Guid.NewGuid();
        using var stream = new MemoryStream([0x01, 0x02, 0x03]);
        var appFile = new AppFile(stream, "image/png");

        _dataStorageSubstitute.Contains<bool>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<bool>(false));

        _s3StorageSubstitute.Save(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<AppFile>(), Arg.Any<CancellationToken>())
            .Returns(UnitResult.Failure(ProblemDetailsHelper.Create("File save failed")));

        var result = await _imageService.Add(entryId, appFile, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.Error.Status);
    }


    [Fact]
    public async Task Add_SaveToStorage_Success()
    {
        var entryId = Guid.NewGuid();
        using var textStream = new MemoryStream([0x01, 0x02, 0x03]);
        var appFile = new AppFile(textStream, "image/png");

        _dataStorageSubstitute.Contains<bool>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<bool>(false));

        _dataStorageSubstitute.Set(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        _s3StorageSubstitute
            .Save(entryId.ToString(), Arg.Any<string>(), appFile, Arg.Any<CancellationToken>())
            .Returns(UnitResult.Success<ProblemDetails>());

        var result = await _imageService.Add(entryId, appFile, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(result.Value.ImageInfo.EntryId, Guid.Empty);
        Assert.NotEqual(result.Value.ImageInfo.Id, Guid.Empty);
        Assert.NotNull(result.Value.ImageInfo.Url);
        Assert.NotNull(result.Value.ClientFileName);
    }

    
    private readonly IS3FileStorage _s3StorageSubstitute;
    private readonly IStringLocalizer<ServerResources> _localizerSubstitute;
    private readonly IDataStorage _dataStorageSubstitute;
    private readonly ImageService _imageService;
}
