using System.IO.Hashing;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Creators;
using Warp.WebApp.Models.Images;
using Warp.WebApp.Services.Entries;
using Warp.WebApp.Services.Images;

namespace Warp.WebApp.Services;

// TODO: consider generic approach to cache key building
public static class CacheKeyBuilder
{
    public static string BuildCreatorCacheKey(in Guid id)
        => $"{nameof(Creator)}::{id}";


    public static string BuildCreatorsEntrySetCacheKey(in Guid id)
        => $"{nameof(Dictionary<Creator, List<Entry>>)}::{id}";


    public static string BuildEntryInfoCacheKey(in Guid entryId)
        => $"{nameof(EntryInfo)}::{entryId}";


    public static string BuildImageHashCacheKey(in Guid entryId, string hash)
        => $"{nameof(ImageService)}::{entryId}::{hash}";


    public static string BuildImageToHashBindingCacheKey(in Guid imageId)
        => $"{nameof(ImageService)}::{typeof(XxHash128)}::{imageId}";


    public static string BuildImageInfoCacheKey(in Guid id)
        => $"{nameof(ImageInfo)}::{id}";


    public static string BuildImageInfoListCacheKey(in Guid id)
        => $"{nameof(List<ImageInfo>)}::{id}";


    public static string BuildReportServiceCacheKey(in Guid id)
        => $"{nameof(ReportService)}::{typeof(Entry)}::{id}";


    public static string BuildViewCountServiceCacheKey(in Guid id)
        => $"{nameof(ViewCountService)}::{typeof(Entry)}::{id}";
}
