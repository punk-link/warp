using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using NSubstitute;
using System.Net;
using Warp.WebApp.Data.S3;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models.Files;
using Warp.WebApp.Services.Images;

namespace Warp.WebApp.Tests;

public class ImageServiceTests
{
    public ImageServiceTests()
    {
        _imageService = new ImageService(_localizerSubstitute, _s3StorageSubstitute);
    }

    [Fact]
    public async Task Add_SaveToStorage_Failed()
    {
        var entryId = Guid.NewGuid();
        using var stream = new MemoryStream(new byte[] { 0x01, 0x02, 0x03 });
        var appFile = new AppFile(stream, "image/png");

        _s3StorageSubstitute
            .Save(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<AppFile>(), Arg.Any<CancellationToken>())
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
        using var textStream = new MemoryStream(new byte[] { 0x01, 0x02, 0x03 });
        var appFile = new AppFile(textStream, "image/png");

        _s3StorageSubstitute
            .Save(entryId.ToString(), Arg.Any<string>(), appFile, Arg.Any<CancellationToken>())
            .Returns(UnitResult.Success<ProblemDetails>());

        var result = await _imageService.Add(entryId, appFile, CancellationToken.None);

        Assert.NotEqual(result.Value.ImageInfo.EntryId, Guid.Empty);
        Assert.NotEqual(result.Value.ImageInfo.Id, Guid.Empty);
        Assert.NotNull(result.Value.ImageInfo.Url);
        Assert.NotNull(result.Value.ClientFileName);
    }

    [Fact]
    public async Task Get_GetFromStorage_Failed()
    {
        var entryId = Guid.NewGuid();
        var imageId = Guid.NewGuid();

        _s3StorageSubstitute
            .Get(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<AppFile, ProblemDetails>(ProblemDetailsHelper.Create("Failed while receiving file.")));

        var result = await _imageService.Get(entryId, imageId, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.Error.Status);
    }

    [Fact]
    public async Task Get_GetFromStorage_Success()
    {
        var entryId = Guid.NewGuid();
        var imageId = Guid.NewGuid();

        using var stream = new MemoryStream(new byte[] { 0x01, 0x02, 0x03 });
        var appFile = new AppFile(stream, "image/png");

        _s3StorageSubstitute
            .Get(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<AppFile, ProblemDetails>(appFile));

        var result = await _imageService.Get(entryId, imageId, CancellationToken.None);

        Assert.Equal(result.Value.Content, appFile.Content);
        Assert.Equal(result.Value.Id, imageId);
        Assert.Equal(result.Value.ContentType, appFile.ContentMimeType);
    }

    [Fact]
    public async Task Get_GetAttached_ContainsFailed()
    {
        var entryId = Guid.NewGuid();
        var imageIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        _s3StorageSubstitute
            .Contains(Arg.Any<string>(), Arg.Any<List<string>>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<HashSet<string>, ProblemDetails>(ProblemDetailsHelper.Create("Error")));

        var result = await _imageService.GetAttached(entryId, imageIds, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.Error.Status);
    }

    [Fact]
    public async Task Get_GetAttached_ContainsSuccess()
    {
        var entryId = Guid.NewGuid();
        var imageIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        _s3StorageSubstitute
            .Contains(Arg.Any<string>(), Arg.Any<List<string>>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<HashSet<string>, ProblemDetails>(new HashSet<string>()));

        var result = await _imageService.GetAttached(entryId, imageIds, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    [Fact]
    public async Task Get_Remove_Success()
    {
        var entryId = Guid.NewGuid();
        var imageId = Guid.NewGuid();

        _s3StorageSubstitute
            .Delete(entryId.ToString(), imageId.ToString(), Arg.Any<CancellationToken>())
            .Returns(UnitResult.Success<ProblemDetails>());

        var result = await _imageService.Remove(entryId, imageId, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Get_Remove_Failed()
    {
        var entryId = Guid.NewGuid();
        var imageId = Guid.NewGuid();

        _s3StorageSubstitute
            .Delete(entryId.ToString(), imageId.ToString(), Arg.Any<CancellationToken>())
            .Returns(UnitResult.Failure<ProblemDetails>(ProblemDetailsHelper.Create("Error")));

        var result = await _imageService.Remove(entryId, imageId, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.Error.Status);
    }

    private readonly IS3FileStorage _s3StorageSubstitute = Substitute.For<IS3FileStorage>();
    private readonly IStringLocalizer<ServerResources> _localizerSubstitute = Substitute.For<IStringLocalizer<ServerResources>>();
    private readonly ImageService _imageService;
}
