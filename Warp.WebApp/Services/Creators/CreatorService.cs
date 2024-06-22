using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Warp.WebApp.Data;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Creators;

namespace Warp.WebApp.Services.Creators;

public class CreatorService : ICreatorService
{
    public CreatorService(IStringLocalizer<ServerResources> localizer, IDataStorage dataStorage)
    {
        _dataStorage = dataStorage;
        _localizer = localizer;
    }


    public async Task<Result<Creator, ProblemDetails>> Add(CancellationToken cancellationToken)
    {
        var creatorId = Guid.NewGuid();
        var creator = new Creator(creatorId);
        
        var userCacheKey = CacheKeyBuilder.BuildCreatorCacheKey(creatorId);
        await _dataStorage.Set(userCacheKey, creator, TimeSpan.FromDays(1), cancellationToken);
        
        return creator;
    }


    public async Task<Result<DummyObject, ProblemDetails>> AttachEntry(Creator creator, Entry entry, TimeSpan expiresIn, CancellationToken cancellationToken)
    {
        var setExpiresIn = expiresIn;
        var entryList = await GetCreatorEntries(creator.Id, cancellationToken);
        if (entryList.Count > 0)
        {
            var maxExpirationDate = entryList.Max(x => x.ExpiresAt);
            setExpiresIn = maxExpirationDate > entry.ExpiresAt
                ? maxExpirationDate - entry.ExpiresAt
                : entry.ExpiresAt - maxExpirationDate;
        }

        var userIdCacheKey = CacheKeyBuilder.BuildCreatorsEntrySetCacheKey(creator.Id);
        var entryCacheKey = CacheKeyBuilder.BuildEntryCacheKey(entry.Id);

        var result = await _dataStorage.AddToSet(userIdCacheKey, entryCacheKey, setExpiresIn, cancellationToken);
        if (!result.IsFailure)
            return new DummyObject();

        var errorMessage = _localizer["AttachEntryErrorMessage"];
        return Result.Failure<DummyObject, ProblemDetails>(ProblemDetailsHelper.CreateServerException(errorMessage));
    }


    public async Task<Creator?> Get(Guid creatorId, CancellationToken cancellationToken)
    {
        var userCacheKey = CacheKeyBuilder.BuildCreatorCacheKey(creatorId);
        var creator = await _dataStorage.TryGet<Creator>(userCacheKey, cancellationToken);
        if (creator == default)
            return null;

        return creator;
    }


    public async Task<Result<Creator, ProblemDetails>> GetOrAdd(Guid? creatorId, CancellationToken cancellationToken)
    {
        if (creatorId is null)
            return await Add(cancellationToken);

        var creator = await Get(creatorId.Value, cancellationToken);
        if (creator.HasValue)
            return creator.Value;

        return await Add(cancellationToken);
    }


    public async Task<Entry?> TryGetUserEntry(Guid userId, Guid entryId, CancellationToken cancellationToken)
    {
        var userIdCacheKey = CacheKeyBuilder.BuildCreatorsEntrySetCacheKey(userId);
        if (!await _dataStorage.IsValueContainsInSet(userIdCacheKey, entryId, cancellationToken))
            return default;

        var entryList = await GetCreatorEntries(userId, cancellationToken);
        var foundEntry = entryList.FirstOrDefault(x => x.Id == entryId);

        return foundEntry;
    }


    public async Task<Result> TryToRemoveUserEntry(Guid userId, Guid entryId, CancellationToken cancellationToken)
    {
        var userIdCacheKey = CacheKeyBuilder.BuildCreatorsEntrySetCacheKey(userId);
        if (!await _dataStorage.IsValueContainsInSet(userIdCacheKey, entryId, cancellationToken))
            return Result.Failure(_localizer["NoEntryRemovePermissionsErrorMessage"]);

        var entryIdCacheKey = CacheKeyBuilder.BuildEntryCacheKey(entryId);
        await _dataStorage.Remove<EntryInfo>(entryIdCacheKey, cancellationToken);
        return Result.Success();
    }


    private async Task<List<Entry>> GetCreatorEntries(Guid creatorId, CancellationToken cancellationToken)
    {
        var userIdCacheKey = CacheKeyBuilder.BuildCreatorsEntrySetCacheKey(creatorId);
        var entryIdList = await _dataStorage.TryGetSet<string>(userIdCacheKey, cancellationToken);
        var entryList = new List<Entry>();
        if (entryIdList is not { Count: > 0 })
            return entryList;

        foreach (var entryId in entryIdList)
        {
            if (!Guid.TryParse(entryId, out var entryGuid))
                continue;
            
            var entryIdCacheKey = CacheKeyBuilder.BuildEntryCacheKey(entryGuid);
            entryList.Add(await _dataStorage.TryGet<Entry>(entryIdCacheKey, cancellationToken));
        }

        return entryList;
    }


    private readonly IDataStorage _dataStorage;
    private readonly IStringLocalizer<ServerResources> _localizer;
}