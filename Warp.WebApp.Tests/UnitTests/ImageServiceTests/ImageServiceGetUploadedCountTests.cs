using Microsoft.Extensions.Options;
using NSubstitute;
using Warp.WebApp.Data;
using Warp.WebApp.Data.S3;
using Warp.WebApp.Models.Options;
using Warp.WebApp.Services.Images;

namespace Warp.WebApp.Tests.UnitTests.ImageServiceTests;

public class ImageServiceGetUploadedCountTests
{
    public ImageServiceGetUploadedCountTests()
    {
        _dataStorageSubstitute = Substitute.For<IDataStorage>();
        _s3StorageSubstitute = Substitute.For<IS3FileStorage>();
        _imageService = new ImageService(_dataStorageSubstitute, _s3StorageSubstitute, Options.Create(new ImageCacheOptions { MaxCachableFileSize = 1_048_576 }));
    }


    [Fact]
    public async Task GetUploadedCount_NoUploads_ReturnsZero()
    {
        var entryId = Guid.NewGuid();

        _dataStorageSubstitute.TryGetSet<Guid>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<HashSet<Guid>>([]));

        var count = await _imageService.GetUploadedCount(entryId, CancellationToken.None);

        Assert.Equal(0, count);
    }


    [Fact]
    public async Task GetUploadedCount_MultipleUploads_ReturnsCorrectCount()
    {
        var entryId = Guid.NewGuid();
        var imageIds = new HashSet<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        _dataStorageSubstitute.TryGetSet<Guid>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<HashSet<Guid>>(imageIds));

        var count = await _imageService.GetUploadedCount(entryId, CancellationToken.None);

        Assert.Equal(3, count);
    }


    private readonly IDataStorage _dataStorageSubstitute;
    private readonly IS3FileStorage _s3StorageSubstitute;
    private readonly ImageService _imageService;
}
