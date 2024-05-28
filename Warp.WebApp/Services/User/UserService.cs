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

        var entryList = await _dataStorage.TryGet<List<Entry>>(userId, cancellationToken);
        if (entryList != null && entryList.Count > 0)
        {
            var maxExpirationDate = entryList.Max(x => x.ExpiresAt);
            listExpiresIn = maxExpirationDate > value.ExpiresAt
                ? maxExpirationDate - value.ExpiresAt
                : value.ExpiresAt - maxExpirationDate;
        }

        return await _dataStorage.Set(userId, value, listExpiresIn, cancellationToken, true);
    }


    public async Task<List<Entry>> TryGetUserEntry(string userId, string entryId, CancellationToken cancellationToken)
    {
        return null;
    }


    private readonly IDataStorage _dataStorage;
}
