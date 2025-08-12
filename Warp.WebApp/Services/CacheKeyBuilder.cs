using System.IO.Hashing;
using Warp.WebApp.Models.Creators;
using Warp.WebApp.Models.Entries;
using Warp.WebApp.Services.Entries;
using Warp.WebApp.Services.Images;

namespace Warp.WebApp.Services;

public static class CacheKeyBuilder
{
    public static string BuildCreatorCacheKey(in Guid id)
        => BuildCacheKey<Creator>(id);


    public static string BuildCreatorsEntrySetCacheKey(in Guid id)
        => BuildCacheKey<Dictionary<Creator, List<Entry>>>(id);


    public static string BuildEntryInfoCacheKey(in Guid entryId)
        => BuildCacheKey<EntryInfo>(entryId);


    public static string BuildEntryOpenGraphDescriptionCacheKey(in Guid entryId)
        => BuildCacheKey<EntryOpenGraphDescription>(entryId);


    public static string BuildImageHashCacheKey(in Guid entryId, string hash)
        => BuildCacheKey<ImageService>(entryId, hash);


    public static string BuildImageToHashBindingCacheKey(in Guid imageId)
        => BuildCacheKey<ImageService>(nameof(XxHash128), imageId);


    public static string BuildReportServiceCacheKey(in Guid id)
        => BuildCacheKey<ReportService>(nameof(Entry), id);


    public static string BuildViewCountServiceCacheKey(in Guid id)
        => BuildCacheKey<ViewCountService>(nameof(Entry), id);


    private static string BuildCacheKey<T>(params object[] identifiers)
        => $"{typeof(T).Name}::{string.Join("::", identifiers)}";
}
