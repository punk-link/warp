using Warp.WebApp.Models;
using Warp.WebApp.Services.Entries;

namespace Warp.WebApp.Services;

public static class CacheKeyBuilder
{
    public static string BuildStringCacheKey(Guid id)
        => $"\"string\"::{id}";


    public static string BuildEntryCacheKey(Guid id)
        => $"{nameof(Entry)}::{id}";


    public static string BuildImageInfoListCacheKey(Guid id)
        => $"{nameof(List<ImageInfo>)}::{id}";


    public static string BuildImageInfoCacheKey(Guid id)
        => $"{nameof(ImageInfo)}::{id}";


    public static string BuildReportServiceCacheKey(in Guid id)
        => $"{nameof(ReportService)}::{typeof(Entry)}::{id}";

    public static string BuildViewCountServiceCacheKey(in Guid id)
        => $"{nameof(ReportService)}::{typeof(Entry)}::{id}";
}
