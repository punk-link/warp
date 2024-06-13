using CSharpFunctionalExtensions;
using Newtonsoft.Json.Linq;
using Warp.WebApp.Data;
using Warp.WebApp.Models;


namespace Warp.WebApp.Services.User;

public class UserService : IUserService
{
    public UserService(IDataStorage dataStorage)
    {
        _dataStorage = dataStorage;
    }


    public async Task<Result> AttachEntryToUser(string userIdCacheKey, string entryCacheKey, Entry value, TimeSpan expiresIn, CancellationToken cancellationToken)
    {
        var listExpiresIn = expiresIn;
        var entryList = await GetUserEntries(userIdCacheKey, cancellationToken);
        if (entryList.Count > 0)
        {
            var maxExpirationDate = entryList.Max(x => x.ExpiresAt);
            listExpiresIn = maxExpirationDate > value.ExpiresAt
                ? maxExpirationDate - value.ExpiresAt
                : value.ExpiresAt - maxExpirationDate;
        }
        
        return await _dataStorage.CrossValueSet(userIdCacheKey, entryCacheKey, listExpiresIn, entryCacheKey, value, expiresIn, cancellationToken);
    }


    public async Task<Entry?> TryGetUserEntry(string userIdCacheKey, Guid entryId, CancellationToken cancellationToken)
    {
        var entryList = await GetUserEntries(userIdCacheKey, cancellationToken);
        var foundEntry = entryList.FirstOrDefault(x => x.Id == entryId);

        return foundEntry;
    }


    public async Task<Result> TryToRemoveUserEntry(string userIdCacheKey, Guid entryId, CancellationToken cancellationToken)
    {
        if (!await _dataStorage.IsValueContainsInList(userIdCacheKey, entryId.ToString(), cancellationToken))
            return Result.Failure("Can`t remove entry cause of no permission.");

        var entryIdCacheKey = CacheKeyBuilder.BuildEntryCacheKey(entryId);
        _dataStorage.Remove<EntryInfo>(entryIdCacheKey, cancellationToken);
        return Result.Success();
    }


    private async Task<List<Entry>> GetUserEntries(string userIdCacheKey, CancellationToken cancellationToken)
    {
        var entryIdList = await _dataStorage.TryGetList<string>(userIdCacheKey, cancellationToken);
        var entryList = new List<Entry>();
        if (entryIdList != null && entryIdList.Count > 0)
        {
            foreach (var entryId in entryIdList)
            {
                Guid.TryParse(entryId, out var entryGuid);
                var entryIdCacheKey = CacheKeyBuilder.BuildEntryCacheKey(entryGuid);
                entryList.Add(await _dataStorage.TryGet<Entry>(entryIdCacheKey, cancellationToken));
            }
        }

        return entryList;
    }

    private readonly IDataStorage _dataStorage;
}
