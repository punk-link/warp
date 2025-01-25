using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Localization;
using Moq;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warp.WebApp.Data;
using Warp.WebApp.Extensions;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Creators;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Creators;

namespace Warp.WebApp.Tests;
public class CreatorServiceTests
{
    public CreatorServiceTests()
    {    
        _creatorService = new CreatorService(_localizerMock.Object, _dataStorageMock.Object);
    }

    [Fact]
    public async Task Add_Default()
    {
        var creator = await _creatorService.Add(CancellationToken.None);
        Assert.NotNull<Creator>(creator);
    }

    [Fact]
    public async Task Get_CreatorIdNotFound_ReturnsProblemDetails()
    {
        var creatorId = Guid.NewGuid();

        _dataStorageMock
            .Setup(x => x.TryGet<Creator>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Creator));

        var result = await _creatorService.Get(creatorId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(400, result.Error.Status);
    }

    [Fact]
    public async Task Get_CreatorIdIsNull_ReturnsProblemDetails()
    {
        var result = await _creatorService.Get(null, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(400, result.Error.Status);
    }

    [Fact]
    public async Task AttachEntry_WithoutExistingEntries_AttachesSuccessfully()
    {
        var creator = new Creator(Guid.NewGuid());
        var entryInfo = new EntryInfo(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.Now, 
            DateTime.Now.AddDays(1), 
            Models.Entries.Enums.EditMode.Text, 
            new Entry("Some content"), 
            new List<ImageInfo>(), 
            new Models.Entries.EntryOpenGraphDescription("Some content", "Some content", null),
            0);

        var userIdCacheKey = CacheKeyBuilder.BuildCreatorsEntrySetCacheKey(creator.Id);

        _dataStorageMock
            .Setup(x => x.TryGetSet<Guid>(userIdCacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<Guid>());

        _dataStorageMock
            .Setup(x => x.AddToSet<Guid>(userIdCacheKey, entryInfo.Id, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _creatorService.AttachEntry(creator, entryInfo, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task AttachEntry_AttachFails_ReturnsProblemDetails()
    {
        var creator = new Creator(Guid.NewGuid());
        var entryInfo = new EntryInfo(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.Now,
            DateTime.Now.AddDays(1),
            Models.Entries.Enums.EditMode.Text,
            new Entry("Some content"),
            new List<ImageInfo>(),
            new Models.Entries.EntryOpenGraphDescription("Some content", "Some content", null),
            0);


        var userIdCacheKey = CacheKeyBuilder.BuildCreatorsEntrySetCacheKey(creator.Id);

        _dataStorageMock.Setup(ds => ds.TryGetSet<Guid>(userIdCacheKey, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new HashSet<Guid>());

        _dataStorageMock.Setup(ds => ds.AddToSet(userIdCacheKey, entryInfo.Id, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Result.Failure("Failure"));

        var result = await _creatorService.AttachEntry(creator, entryInfo, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task AttachEntry_WithExistingEntries_AttachesSuccessfully()
    {
        var creator = new Creator(Guid.NewGuid());
        var entryInfo = new EntryInfo(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.Now,
            DateTime.Now.AddDays(1),
            Models.Entries.Enums.EditMode.Text,
            new Entry("Some content"),
            new List<ImageInfo>(),
            new Models.Entries.EntryOpenGraphDescription("Some content", "Some content", null),
            0);

        var userIdCacheKey = CacheKeyBuilder.BuildCreatorsEntrySetCacheKey(creator.Id);

        var existingEntryIds = new HashSet<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        var existingEntryInfoFirst = new EntryInfo(
            existingEntryIds.First(),
            Guid.NewGuid(),
            DateTime.Now,
            DateTime.Now.AddDays(1),
            Models.Entries.Enums.EditMode.Text,
            new Entry("Some content"),
            new List<ImageInfo>(),
            new Models.Entries.EntryOpenGraphDescription("Some content", "Some content", null),
            0);

        var existingEntryInfoSecond = new EntryInfo(
            existingEntryIds.Last(),
            Guid.NewGuid(),
            DateTime.Now,
            DateTime.Now.AddDays(1),
            Models.Entries.Enums.EditMode.Text,
            new Entry("Some content"),
            new List<ImageInfo>(),
            new Models.Entries.EntryOpenGraphDescription("Some content", "Some content", null),
            0);

        _dataStorageMock.Setup(ds => ds.TryGetSet<Guid>(userIdCacheKey, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(existingEntryIds);

        _dataStorageMock.Setup(ds => ds.TryGet<EntryInfo>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync((string key, CancellationToken _) =>
                        {
                            if (key.Contains(existingEntryInfoFirst.Id.ToString()))
                                return existingEntryInfoFirst;
                            if (key.Contains(existingEntryInfoFirst.Id.ToString()))
                                return existingEntryInfoFirst;
                            return default;
                        });

        _dataStorageMock.Setup(ds => ds.AddToSet(userIdCacheKey, entryInfo.Id, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Result.Success());

        var result = await _creatorService.AttachEntry(creator, entryInfo, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    private readonly Mock<IDataStorage> _dataStorageMock = new();
    private readonly Mock<IStringLocalizer<ServerResources>> _localizerMock = new();
    private readonly CreatorService _creatorService;
}
