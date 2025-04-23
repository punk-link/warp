using Warp.WebApp.Attributes;
using Warp.WebApp.Data;

namespace Warp.WebApp.Services.Entries;

/// <summary>
/// Implementation of <see cref="IViewCountService"/> that manages view counts using a data storage mechanism.
/// </summary>
public sealed class ViewCountService : IViewCountService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ViewCountService"/> class.
    /// </summary>
    /// <param name="dataStorage">The data storage service used for managing view counts.</param>
    public ViewCountService(IDataStorage dataStorage)
    {
        _dataStorage = dataStorage;
    }


    /// <inheritdoc cref="IViewCountService.AddAndGet"/>
    [TraceMethod]
    public async Task<long> AddAndGet(Guid itemId, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeyBuilder.BuildViewCountServiceCacheKey(in itemId);
        return await _dataStorage.AddAndGetCounter(cacheKey, cancellationToken);
    }


    /// <inheritdoc cref="IViewCountService.Get"/>
    [TraceMethod]
    public async Task<long> Get(Guid itemId, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeyBuilder.BuildViewCountServiceCacheKey(in itemId);
        return await _dataStorage.TryGet<long>(cacheKey, cancellationToken);
    }


    private readonly IDataStorage _dataStorage;
}