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
        var maxExpirationDate = await GetSetExpirationTime(creator.Id, cancellationToken);
        var setExpiresIn = maxExpirationDate > entry.ExpiresAt
                ? maxExpirationDate - entry.ExpiresAt
                : entry.ExpiresAt - maxExpirationDate;

        var userIdCacheKey = CacheKeyBuilder.BuildCreatorsEntrySetCacheKey(creator.Id);

        var result = await _dataStorage.AddToSet(userIdCacheKey, entry.Id, setExpiresIn, cancellationToken);
        if (!result.IsFailure)
            return new DummyObject();

        var errorMessage = _localizer["AttachEntryErrorMessage"];
        return Result.Failure<DummyObject, ProblemDetails>(ProblemDetailsHelper.CreateServerException(errorMessage));
    }


    public async Task<Creator?> Get(Guid? creatorId, CancellationToken cancellationToken)
    {
        if (creatorId is null)
            return null;

        var userCacheKey = CacheKeyBuilder.BuildCreatorCacheKey(creatorId.Value);
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


    public Task<bool> IsEntryBelongsToCreator(Creator creator, Guid entryId, CancellationToken cancellationToken)
        => IsEntryBelongsToCreator(creator.Id, entryId, cancellationToken);


    public async Task<bool> IsEntryBelongsToCreator(Guid creatorId, Guid entryId, CancellationToken cancellationToken)
    {
        var creatorsEntrySetCacheKey = CacheKeyBuilder.BuildCreatorsEntrySetCacheKey(creatorId);
        return await _dataStorage.ContainsInSet(creatorsEntrySetCacheKey, entryId, cancellationToken);
    }


    private async Task<DateTime> GetSetExpirationTime(Guid creatorId, CancellationToken cancellationToken)
    {
        var userIdCacheKey = CacheKeyBuilder.BuildCreatorsEntrySetCacheKey(creatorId);
        var entryIds = await _dataStorage.TryGetSet<Guid>(userIdCacheKey, cancellationToken);
        if (entryIds.Count == 0)
            return DateTime.MinValue;
        
        var expirationDates = new List<DateTime>(entryIds.Count);
        foreach (var entryId in entryIds)
        {
            var entryIdCacheKey = CacheKeyBuilder.BuildEntryCacheKey(entryId);

            // TODO: requesting all entries one by one is suboptimal. Consider refactoring.
            var entry = await _dataStorage.TryGet<Entry>(entryIdCacheKey, cancellationToken);
            expirationDates.Add(entry.ExpiresAt);
        }

        return expirationDates.Max();
    }


    private readonly IDataStorage _dataStorage;
    private readonly IStringLocalizer<ServerResources> _localizer;
}