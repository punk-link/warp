﻿using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warp.WebApp.Data.S3;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Files;
using Warp.WebApp.Services.Images;

namespace Warp.WebApp.Tests;
public class ImageServiceTest
{
    public ImageServiceTest()
    {
        _imageService = new ImageService(_localizerMock.Object, _s3StorageMock.Object);
    }

    [Fact]
    public async Task Add_SaveToStorage_Failed()
    {
        var entryId = Guid.NewGuid();
        using var textStream = new MemoryStream(new byte[] { 0x01, 0x02, 0x03 });
        var appFile = new AppFile(textStream, "image/png");

        _s3StorageMock
            .Setup(x => x.Save(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AppFile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(UnitResult.Failure(ProblemDetailsHelper.Create("File save failed")));

        var result = await _imageService.Add(entryId, appFile, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("File save failed", result.Error.Detail);
    }

    [Fact]
    public async Task Add_SaveToStorage_Success()
    {
        var entryId = Guid.NewGuid();
        using var textStream = new MemoryStream(new byte[] { 0x01, 0x02, 0x03 });
        var appFile = new AppFile(textStream, "image/png");

        _s3StorageMock
            .Setup(x => x.Save(entryId.ToString(), It.IsAny<string>(), appFile, It.IsAny<CancellationToken>()))
            .ReturnsAsync(UnitResult.Success<ProblemDetails>());

        var result = await _imageService.Add(entryId, appFile, CancellationToken.None);

        Assert.NotEqual(result.Value.ImageInfo.EntryId, Guid.Empty);
        Assert.NotEqual(result.Value.ImageInfo.Id, Guid.Empty);
        Assert.NotNull(result.Value.ImageInfo.Url);
        Assert.NotNull(result.Value.ClientFileName);
    }

    private readonly Mock<IS3FileStorage> _s3StorageMock = new();
    private readonly Mock<IStringLocalizer<ServerResources>> _localizerMock = new();
    private readonly ImageService _imageService;
}
