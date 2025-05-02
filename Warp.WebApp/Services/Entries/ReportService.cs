using Warp.WebApp.Attributes;
using Warp.WebApp.Constants.Caching;
using Warp.WebApp.Data;
using Warp.WebApp.Models;

namespace Warp.WebApp.Services.Entries;

public sealed class ReportService : IReportService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReportService"/> class.
    /// </summary>
    /// <param name="dataStorage">The data storage service used for managing reports.</param>
    public ReportService(IDataStorage dataStorage)
    {
        _dataStorage = dataStorage;
    }


    /// <inheritdoc cref="IReportService.Contains"/>"/>
    [TraceMethod]
    public ValueTask<bool> Contains(Guid id, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeyBuilder.BuildReportServiceCacheKey(in id);
        return _dataStorage.Contains<Report>(cacheKey, cancellationToken);
    }


    /// <inheritdoc cref="IReportService.MarkAsReported"/>"/>
    [TraceMethod]
    public Task MarkAsReported(Guid id, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeyBuilder.BuildReportServiceCacheKey(in id);
        return _dataStorage.Set(cacheKey, new Report(id), CachingConstants.MaxSupportedCachingTime, cancellationToken);
    }


    private readonly IDataStorage _dataStorage;
}