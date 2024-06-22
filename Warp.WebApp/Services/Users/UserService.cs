using CSharpFunctionalExtensions;
using Microsoft.Extensions.Localization;
using System.Threading.Tasks;
using Warp.WebApp.Data;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Users;

namespace Warp.WebApp.Services.Users;

public class UserService : IUserService
{
    public UserService(IStringLocalizer<ServerResources> localizer, IDataStorage dataStorage)
    {
        _dataStorage = dataStorage;
        _localizer = localizer;
    }


    public async Task<Result> AttachEntryToUser(Guid userId, Entry value, TimeSpan expiresIn, CancellationToken cancellationToken)
    {
        var listExpiresIn = expiresIn;
        var entryList = await GetUserEntries(userId, cancellationToken);
        if (entryList.Count > 0)
        {
            var maxExpirationDate = entryList.Max(x => x.ExpiresAt);
            listExpiresIn = maxExpirationDate > value.ExpiresAt
                ? maxExpirationDate - value.ExpiresAt
                : value.ExpiresAt - maxExpirationDate;
        }
        var userIdCacheKey = CacheKeyBuilder.BuildSetGuidCacheKey(userId);
        var entryCacheKey = CacheKeyBuilder.BuildEntryCacheKey(value.Id);

        return await _dataStorage.CrossValueSet(userIdCacheKey, value.Id, listExpiresIn, entryCacheKey, value, expiresIn, cancellationToken);
    }


    public async Task<Entry?> TryGetUserEntry(Guid userId, Guid entryId, CancellationToken cancellationToken)
    {
        var userIdCacheKey = CacheKeyBuilder.BuildSetGuidCacheKey(userId);
        if (!await _dataStorage.IsValueContainsInSet(userIdCacheKey, entryId, cancellationToken))
            return default;

        var entryList = await GetUserEntries(userId, cancellationToken);
        var foundEntry = entryList.FirstOrDefault(x => x.Id == entryId);

        return foundEntry;
    }


    public async Task<Result> TryToRemoveUserEntry(Guid userId, Guid entryId, CancellationToken cancellationToken)
    {
        var userIdCacheKey = CacheKeyBuilder.BuildSetGuidCacheKey(userId);
        if (!await _dataStorage.IsValueContainsInSet(userIdCacheKey, entryId, cancellationToken))
            return Result.Failure(_localizer["NoEntryRemovePermissionsErrorMessage"]);

        var entryIdCacheKey = CacheKeyBuilder.BuildEntryCacheKey(entryId);
        await _dataStorage.Remove<EntryInfo>(entryIdCacheKey, cancellationToken);
        return Result.Success();
    }


    private async Task<List<Entry>> GetUserEntries(Guid userId, CancellationToken cancellationToken)
    {
        var userIdCacheKey = CacheKeyBuilder.BuildSetGuidCacheKey(userId);
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