using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Warp.WebApp.Models;

namespace Warp.WebApp.Services;

public class ImageService : IImageService
{
    public ImageService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }


    public async Task<List<(string, Guid)>> Add(List<IFormFile> files)
    {
        var results = new List<(string, Guid)>(files.Count);
        foreach (var file in files)
        {
            var result = await Add(file);
            results.Add(result);
        }

        return results;
    }


    public async Task<(string, Guid)> Add(IFormFile file)
    {
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);

        var entry = new ImageEntry
        {
            Id = Guid.NewGuid(),
            Content = JsonSerializer.SerializeToUtf8Bytes(memoryStream)
        };

        var cacheKey = BuildEntryCacheKey(entry.Id);
        _memoryCache.Set(cacheKey, entry, DateTimeOffset.Now.AddHours(1));

        return (file.FileName, entry.Id);
    }


    public void Attach(Guid entryId, DateTimeOffset absoluteExpirationTime, List<Guid> imageIds)
    {
        var imageEntries = new List<ImageEntry>(imageIds.Count);
        foreach (var imageId in imageIds)
        {
            var entryCacheKey = BuildEntryCacheKey(imageId);
            if (_memoryCache.TryGetValue(entryCacheKey, out ImageEntry value))
                imageEntries.Add(value);
        }

        var bucket = new ImageBucket
        {
            Id = Guid.NewGuid(),
            Images = imageEntries
        };

        var bucketCacheKey = BuildBucketCacheKey(entryId);
        _memoryCache.Set(bucketCacheKey, bucket, absoluteExpirationTime);

        foreach (var entry in imageEntries)
        {
            var entryCacheKey = BuildEntryCacheKey(entry.Id);
            _memoryCache.Remove(entryCacheKey);
        }
    }


    public List<ImageEntry> Get(Guid entryId)
    {
        var bucketCacheKey = BuildBucketCacheKey(entryId);
        if (_memoryCache.TryGetValue(bucketCacheKey, out List<ImageEntry>? values))
            return values ?? Enumerable.Empty<ImageEntry>().ToList();

        return Enumerable.Empty<ImageEntry>().ToList();
    }


    private static string BuildBucketCacheKey(Guid id)
        => $"{nameof(ImageBucket)}::{id}";


    private static string BuildEntryCacheKey(Guid id)
        => $"{nameof(ImageEntry)}::{id}";


    private readonly IMemoryCache _memoryCache;
}