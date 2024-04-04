using Warp.WebApp.Data;
using Warp.WebApp.Models;

namespace Warp.WebApp.Services.Entries;

public sealed class ViewCountService : IViewCountService
{
    public ViewCountService(IDataStorage dataStorage)
    {
        _dataStorage = dataStorage;
    }


    public async Task<long> AddAndGet(Guid itemId, CancellationToken cancellationToken)
    {
        var cacheKey = GetCacheKey(in itemId);
        return await _dataStorage.AddAndGetCounter(cacheKey, cancellationToken);


        static string GetCacheKey(in Guid id)
            => $"{nameof(ViewCountService)}::{typeof(Entry)}::{id}";
    }


    private readonly IDataStorage _dataStorage;
    private static readonly Dictionary<Guid, int> Counter = [];
}