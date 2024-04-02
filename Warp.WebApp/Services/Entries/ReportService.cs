using Warp.WebApp.Constants.Caching;
using Warp.WebApp.Data;
using Warp.WebApp.Models;

namespace Warp.WebApp.Services.Entries;

public sealed class ReportService : IReportService
{
    public ReportService(IDataStorage dataStorage)
    {
        _dataStorage = dataStorage;
    }


    public ValueTask<bool> Contains(Guid id)
    {
        var cacheKey = GetCacheKey(in id);
        return _dataStorage.Contains<Report>(cacheKey);
    }


    public Task MarkAsReported(Guid id, CancellationToken cancellationToken)
    {
        var cacheKey = GetCacheKey(in id);
        return _dataStorage.Set(cacheKey, new Report(id), CachingConstants.MaxSupportedCachingTime, cancellationToken);
    }


    private static string GetCacheKey(in Guid id)
        => $"{nameof(ReportService)}::{typeof(Entry)}::{id}";


    private readonly IDataStorage _dataStorage;
}