using CSharpFunctionalExtensions;
using Warp.WebApp.Data;
using Warp.WebApp.Models;


namespace Warp.WebApp.Services.User;

public class UserService : IUserService
{
    public UserService(IDataStorage dataStorage)
    {
        _dataStorage = dataStorage;
    }


    public async Task<Result> AttachEntryToUser(string userId, Entry value, TimeSpan expiresIn, CancellationToken cancellationToken)
    {
        var listExpiresIn = expiresIn;

        var entryIdList = await _dataStorage.TryGet<List<string>>(userId, cancellationToken);
        var entryList = new List<Entry>();
        if (entryIdList != null && entryIdList.Count > 0)
        {
            foreach (var entryId in entryIdList)
                entryList.Add(await _dataStorage.TryGet<Entry>(entryId, cancellationToken));

            var maxExpirationDate = entryList.Max(x => x.ExpiresAt);
            listExpiresIn = maxExpirationDate > value.ExpiresAt
                ? maxExpirationDate - value.ExpiresAt
                : value.ExpiresAt - maxExpirationDate;
        }
        
        return await _dataStorage.CrossValueSet(userId, value.Id.ToString(), listExpiresIn, value.Id.ToString(), value, expiresIn, cancellationToken);
    }


    public async Task<Result> TryGetUserEntry(string userId, Guid entryId, CancellationToken cancellationToken)
    {
        var entryList = await _dataStorage.TryGet<List<Entry>>(userId, cancellationToken);
        var foundEntry = entryList != null ? entryList?.FirstOrDefault(x => x.Id == entryId) : null;

        return foundEntry != null
            ? Result.Success(foundEntry)
            : Result.Failure("Selected entry is not found for this user.");
    }


    private readonly IDataStorage _dataStorage;
}
