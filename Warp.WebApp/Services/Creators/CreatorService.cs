using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Warp.WebApp.Attributes;
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


    [TraceMethod]
    public async Task<Creator> Add(CancellationToken cancellationToken)
    {
        var creatorId = Guid.NewGuid();
        var creator = new Creator(creatorId);
        
        var userCacheKey = CacheKeyBuilder.BuildCreatorCacheKey(creatorId);
        await _dataStorage.Set(userCacheKey, creator, TimeSpan.FromDays(365), cancellationToken);
        
        return creator;
    }


    [TraceMethod]
    public async Task<UnitResult<ProblemDetails>> AttachEntry(Creator creator, EntryInfo entryInfo, CancellationToken cancellationToken)
    {
        var userIdCacheKey = CacheKeyBuilder.BuildCreatorsEntrySetCacheKey(creator.Id);

        return await UnitResult.Success<ProblemDetails>()
            .Map(GetMaxExpirationDateTime)
            .Bind(GetExpirationSpan)
            .Bind(AttachEntryToCreator);


        async Task<DateTime> GetMaxExpirationDateTime()
        {
            var entryInfoIds = await _dataStorage.TryGetSet<Guid>(userIdCacheKey, cancellationToken);
            if (entryInfoIds.Count == 0)
                return DateTime.MinValue;

            var expirationDates = new List<DateTime>(entryInfoIds.Count);
            foreach (var entryInfoId in entryInfoIds)
            {
                var entryIdCacheKey = CacheKeyBuilder.BuildEntryInfoCacheKey(entryInfoId);

                // TODO: requesting all entries one by one is suboptimal. Consider refactoring.
                var entryInfo = await _dataStorage.TryGet<EntryInfo>(entryIdCacheKey, cancellationToken);
                expirationDates.Add(entryInfo.ExpiresAt);
            }

            return expirationDates.Max();
        }


        Result<TimeSpan, ProblemDetails> GetExpirationSpan(DateTime maxExpirationDateTime) 
            => maxExpirationDateTime > entryInfo.ExpiresAt
                ? maxExpirationDateTime - entryInfo.ExpiresAt
                : entryInfo.ExpiresAt - maxExpirationDateTime;


        async Task<UnitResult<ProblemDetails>> AttachEntryToCreator(TimeSpan expirationSpan)
        {
            var result = await _dataStorage.AddToSet(userIdCacheKey, entryInfo.Id, expirationSpan, cancellationToken);
            if (result.IsSuccess)
                return UnitResult.Success<ProblemDetails>();

            var errorMessage = _localizer["AttachEntryErrorMessage"];
            return UnitResult.Failure(ProblemDetailsHelper.CreateServerException(errorMessage));
        }
    }


    [TraceMethod]
    public async Task<Result<Creator, ProblemDetails>> Get(Guid? creatorId, CancellationToken cancellationToken)
    {
        // TODO: consider to log creator errors as a warning
        if (creatorId is null)
            return ProblemDetailsHelper.Create(_localizer["CreatorIdIsNull"]);

        var userCacheKey = CacheKeyBuilder.BuildCreatorCacheKey(creatorId.Value);
        var creator = await _dataStorage.TryGet<Creator>(userCacheKey, cancellationToken);
        if (creator == default)
            return ProblemDetailsHelper.Create(_localizer["CreatorIdIsNotFound"]);

        return creator;
    }


    public async Task<Creator> GetOrAdd(Guid? creatorId, CancellationToken cancellationToken)
    {
        if (creatorId is null)
            return await Add(cancellationToken);

        var (isSuccess, _, creator, _) = await Get(creatorId, cancellationToken);
        if (isSuccess)
            return creator;
            
        return await Add(cancellationToken);
    }


    private readonly IDataStorage _dataStorage;
    private readonly IStringLocalizer<ServerResources> _localizer;
}