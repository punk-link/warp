using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Data;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;

namespace Warp.WebApp.Services;

public class ImageService : IImageService
{
    public ImageService(IDataStorage dataStorage)
    {
        _dataStorage = dataStorage;
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


    public async Task Attach(Guid entryId, TimeSpan relativeExpirationTime, List<Guid> imageIds)
    {
        if (imageIds.Count == 0)
            return;

        var imageEntries = new List<ImageEntry>(imageIds.Count);
        foreach (var imageId in imageIds)
        {
            var entryCacheKey = BuildEntryCacheKey(imageId);
            var value = await _dataStorage.TryGet<ImageEntry>(entryCacheKey);
            if (!value.Equals(default))
                imageEntries.Add(value);
        }

        var bucketCacheKey = BuildBucketCacheKey(entryId);
        await _dataStorage.Set(bucketCacheKey, imageEntries, relativeExpirationTime);

        foreach (var entry in imageEntries)
        {
            var entryCacheKey = BuildEntryCacheKey(entry.Id);
            _dataStorage.Remove<List<ImageEntry>>(entryCacheKey);
        }
    }


    public async Task<List<ImageEntry>> Get(Guid entryId)
    {
        var bucketCacheKey = BuildBucketCacheKey(entryId);
        var values = await _dataStorage.TryGet<List<ImageEntry>>(bucketCacheKey);

        return values ?? Enumerable.Empty<ImageEntry>().ToList();
    }


    public async Task<Result<ImageEntry, ProblemDetails>> Get(Guid entryId, Guid imageId)
    {
        var images = await Get(entryId);
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
        await _dataStorage.Set(cacheKey, entry, TimeSpan.FromHours(1));

        return (file.FileName, entry.Id);
    }


    private readonly IDataStorage _dataStorage;
}