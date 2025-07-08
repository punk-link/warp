using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Warp.WebApp.Data;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Creators;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Models.Images;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Creators;

namespace Warp.WebApp.Tests;

public class CreatorServiceTests
{
    public CreatorServiceTests()
    {
        _creatorService = new CreatorService(_loggerFactorySubstitute, _dataStorageSubstitute);
        _creator = new Creator(Guid.NewGuid());
    }

    [Fact]
    public async Task Add_Default()
    {
        var creator = await _creatorService.Add(CancellationToken.None);
        Assert.NotEqual(creator.Id, Guid.Empty);
    }

    [Fact]
    public async Task Get_CreatorIdNotFound_ReturnsDomainError()
    {
        _dataStorageSubstitute
            .TryGet<Creator>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(default(Creator));

        var result = await _creatorService.Get(_creator.Id, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(Constants.Logging.LogEvents.CreatorIdIsNotFound, result.Error.Code);
    }

    [Fact]
    public async Task Get_CreatorIdIsNull_ReturnsDomainError()
    {
        var result = await _creatorService.Get(null, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(Constants.Logging.LogEvents.CreatorIdIsNull, result.Error.Code);
    }

    [Fact]
    public async Task AttachEntry_WithoutExistingEntries_AttachesSuccessfully()
    {
        var entryInfo = new EntryInfo(
            id: Guid.NewGuid(),
            creatorId: Guid.NewGuid(),
            createdAt: DateTime.UtcNow,
            expiresAt: DateTime.UtcNow.AddDays(1),
            editMode: Models.Entries.Enums.EditMode.Simple,
            entry: new Entry("Some content"),
            imageInfos: new List<ImageInfo>(),
            openGraphDescription: new Models.Entries.EntryOpenGraphDescription(
                title: "Some content", 
                description: "Some content", 
                imageUrl: null),
            viewCount: 0);

        var userIdCacheKey = CacheKeyBuilder.BuildCreatorsEntrySetCacheKey(_creator.Id);

        _dataStorageSubstitute
            .TryGetSet<Guid>(userIdCacheKey, Arg.Any<CancellationToken>())
            .Returns(new HashSet<Guid>());

        _dataStorageSubstitute
            .AddToSet<Guid>(userIdCacheKey, entryInfo.Id, Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(UnitResult.Success<DomainError>());

        var result = await _creatorService.AttachEntry(_creator, entryInfo, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task AttachEntry_AttachFails_ReturnsDomainError()
    {
        var entryInfo = new EntryInfo(
            id: Guid.NewGuid(),
            creatorId: Guid.NewGuid(),
            createdAt: DateTime.UtcNow,
            expiresAt: DateTime.UtcNow.AddDays(1),
            editMode: Models.Entries.Enums.EditMode.Simple,
            entry: new Entry("Some content"),
            imageInfos: new List<ImageInfo>(),
            openGraphDescription: new Models.Entries.EntryOpenGraphDescription(
                title: "Some content", 
                description: "Some content", 
                imageUrl: null),
            viewCount: 0);

        var userIdCacheKey = CacheKeyBuilder.BuildCreatorsEntrySetCacheKey(_creator.Id);

        _dataStorageSubstitute
            .TryGetSet<Guid>(userIdCacheKey, Arg.Any<CancellationToken>())
            .Returns(new HashSet<Guid>());

        _dataStorageSubstitute
            .AddToSet(userIdCacheKey, entryInfo.Id, Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(UnitResult.Failure(DomainErrors.DefaultCacheValueError("test")));

        var result = await _creatorService.AttachEntry(_creator, entryInfo, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(Constants.Logging.LogEvents.CantAttachEntryToCreator, result.Error.Code);
    }

    [Fact]
    public async Task AttachEntry_WithExistingEntries_AttachesSuccessfully()
    {
        var entryInfo = new EntryInfo(
            id: Guid.NewGuid(),
            creatorId: Guid.NewGuid(),
            createdAt: DateTime.UtcNow,
            expiresAt: DateTime.UtcNow.AddDays(1),
            editMode: Models.Entries.Enums.EditMode.Simple,
            entry: new Entry("Some content"),
            imageInfos: new List<ImageInfo>(),
            openGraphDescription: new Models.Entries.EntryOpenGraphDescription(
                title: "Some content", 
                description: "Some content", 
                imageUrl: null),
            viewCount: 0);

        var userIdCacheKey = CacheKeyBuilder.BuildCreatorsEntrySetCacheKey(_creator.Id);

        var existingEntryIds = new HashSet<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        var existingEntryInfoFirst = new EntryInfo(
            id: existingEntryIds.First(),
            creatorId: Guid.NewGuid(),
            createdAt: DateTime.UtcNow,
            expiresAt: DateTime.UtcNow.AddDays(1),
            editMode: Models.Entries.Enums.EditMode.Simple,
            entry: new Entry("Some content"),
            imageInfos: new List<ImageInfo>(),
            openGraphDescription: new Models.Entries.EntryOpenGraphDescription(
                title: "Some content", 
                description: "Some content", 
                imageUrl: null),
            viewCount: 0);

        var existingEntryInfoSecond = new EntryInfo(
            id: existingEntryIds.Last(),
            creatorId: Guid.NewGuid(),
            createdAt: DateTime.UtcNow,
            expiresAt: DateTime.UtcNow.AddDays(1),
            editMode: Models.Entries.Enums.EditMode.Simple,
            entry: new Entry("Some content"),
            imageInfos: new List<ImageInfo>(),
            openGraphDescription: new Models.Entries.EntryOpenGraphDescription(
                title: "Some content", 
                description: "Some content", 
                imageUrl: null),
            viewCount: 0);

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
            .Returns(UnitResult.Success<DomainError>());

        var result = await _creatorService.AttachEntry(_creator, entryInfo, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    private readonly IDataStorage _dataStorageSubstitute = Substitute.For<IDataStorage>();
    private readonly ILoggerFactory _loggerFactorySubstitute = Substitute.For<ILoggerFactory>();
    private readonly CreatorService _creatorService;
    private readonly Creator _creator;
}