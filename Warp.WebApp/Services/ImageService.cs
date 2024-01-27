using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;

namespace Warp.WebApp.Services;

public class ImageService : IImageService
{
    public ImageService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }


    public async Task<Dictionary<string, Guid>> Add(List<IFormFile> files)
    {
        // TODO: add validation and a count check

        var results = new Dictionary<string, Guid>(files.Count);
        foreach (var file in files)
        {
            var (clientFileName, id) = await Add(file);
            results.Add(clientFileName, id);
        }

        return results;
    }


    public void Attach(Guid entryId, TimeSpan relativeExpirationTime, List<Guid> imageIds)
    {
        if (imageIds.Count == 0)
            return;

        var imageEntries = new List<ImageEntry>(imageIds.Count);
        foreach (var imageId in imageIds)
        {
            var entryCacheKey = BuildEntryCacheKey(imageId);
            if (_memoryCache.TryGetValue(entryCacheKey, out ImageEntry value))
                imageEntries.Add(value);
        }

        var bucketCacheKey = BuildBucketCacheKey(entryId);
        _memoryCache.Set(bucketCacheKey, imageEntries, relativeExpirationTime);

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


    public Result<ImageEntry, ProblemDetails> Get(Guid entryId, Guid imageId)
    {
        var images = Get(entryId);
        var image = images.FirstOrDefault(x => x.Id == imageId);

        return image != default
            ? Result.Success<ImageEntry, ProblemDetails>(image)
            : ResultHelper.NotFound<ImageEntry>();
    }


    private static string BuildBucketCacheKey(Guid id)
        => $"{nameof(List<ImageEntry>)}::{id}";


    private static string BuildEntryCacheKey(Guid id)
        => $"{nameof(ImageEntry)}::{id}";


    private async Task<(string, Guid)> Add(IFormFile file)
    {
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);

        var entry = new ImageEntry
        {
            Id = Guid.NewGuid(),
            Content = memoryStream.ToArray(),
            ContentType = file.ContentType
        };

        var cacheKey = BuildEntryCacheKey(entry.Id);
        _memoryCache.Set(cacheKey, entry, DateTimeOffset.Now.AddHours(1));

        return (file.FileName, entry.Id);
    }


    private readonly IMemoryCache _memoryCache;
}