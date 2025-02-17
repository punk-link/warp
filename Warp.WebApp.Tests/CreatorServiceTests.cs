using CSharpFunctionalExtensions;
using Microsoft.Extensions.Localization;
using NSubstitute;
using System.Net;
using Xunit;
using Warp.WebApp.Data;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Creators;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Creators;

namespace Warp.WebApp.Tests;

public class CreatorServiceTests
{
    public CreatorServiceTests()
    {
        _creatorService = new CreatorService(_localizerSubstitute, _dataStorageSubstitute);
        _creator = new Creator(Guid.NewGuid());
    }

    [Fact]
    public async Task Add_Default()
    {
        var creator = await _creatorService.Add(CancellationToken.None);
        Assert.NotNull(creator);
    }

    [Fact]
    public async Task Get_CreatorIdNotFound_ReturnsProblemDetails()
    {
        _dataStorageSubstitute
            .TryGet<Creator>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(default(Creator));

        var result = await _creatorService.Get(_creator.Id, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.Error.Status);
    }

    [Fact]
    public async Task Get_CreatorIdIsNull_ReturnsProblemDetails()
    {
        var result = await _creatorService.Get(null, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.Error.Status);
    }

    [Fact]
    public async Task AttachEntry_WithoutExistingEntries_AttachesSuccessfully()
    {
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

        var userIdCacheKey = CacheKeyBuilder.BuildCreatorsEntrySetCacheKey(_creator.Id);

        _dataStorageSubstitute
            .TryGetSet<Guid>(userIdCacheKey, Arg.Any<CancellationToken>())
            .Returns(new HashSet<Guid>());

        _dataStorageSubstitute
            .AddToSet<Guid>(userIdCacheKey, entryInfo.Id, Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _creatorService.AttachEntry(_creator, entryInfo, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task AttachEntry_AttachFails_ReturnsProblemDetails()
    {
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

        var userIdCacheKey = CacheKeyBuilder.BuildCreatorsEntrySetCacheKey(_creator.Id);

        _dataStorageSubstitute
            .TryGetSet<Guid>(userIdCacheKey, Arg.Any<CancellationToken>())
            .Returns(new HashSet<Guid>());

        _dataStorageSubstitute
            .AddToSet(userIdCacheKey, entryInfo.Id, Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Failure"));

        var result = await _creatorService.AttachEntry(_creator, entryInfo, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal((int)HttpStatusCode.InternalServerError, result.Error.Status);
    }

    [Fact]
    public async Task AttachEntry_WithExistingEntries_AttachesSuccessfully()
    {
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

        var userIdCacheKey = CacheKeyBuilder.BuildCreatorsEntrySetCacheKey(_creator.Id);

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

        _dataStorageSubstitute
            .TryGetSet<Guid>(userIdCacheKey, Arg.Any<CancellationToken>())
            .Returns(existingEntryIds);

        _dataStorageSubstitute
            .TryGet<EntryInfo>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(args =>
            {
                var key = args.Arg<string>();
                if (key.Contains(existingEntryInfoFirst.Id.ToString()))
                    return existingEntryInfoFirst;
                if (key.Contains(existingEntryInfoSecond.Id.ToString()))
                    return existingEntryInfoSecond;
                return default;
            });

        _dataStorageSubstitute
            .AddToSet(userIdCacheKey, entryInfo.Id, Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _creatorService.AttachEntry(_creator, entryInfo, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    private readonly IDataStorage _dataStorageSubstitute = Substitute.For<IDataStorage>();
    private readonly IStringLocalizer<ServerResources> _localizerSubstitute = Substitute.For<IStringLocalizer<ServerResources>>();
    private readonly CreatorService _creatorService;
    private readonly Creator _creator;
}