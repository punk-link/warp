using CSharpFunctionalExtensions;
using Warp.WebApp.Data;


namespace Warp.WebApp.Services.Entries;

public class UserService : IUserService
{
    public UserService(IDataStorage dataStorage)
    {
        _dataStorage = dataStorage;    
    }


    public async Task<Result> Set<Entry>(string userId, Models.Entry value, TimeSpan expiresIn, CancellationToken cancellationToken)
    {
        var entryList = await _dataStorage.TryGet<List<Models.Entry>>(userId, cancellationToken);
        if (entryList != null && entryList.Count > 0) 
        {
            var maxExpirationDate = entryList.Max(x => x.ExpiresAt);
            var expiresInForList = maxExpirationDate > value.ExpiresAt ? maxExpirationDate - value.ExpiresAt : value.ExpiresAt - maxExpirationDate;
            return await _dataStorage.Set(userId, value, expiresInForList, cancellationToken);
        }

        return await _dataStorage.Set(userId, value, expiresIn, cancellationToken);
    }

    private readonly IDataStorage _dataStorage;
}
