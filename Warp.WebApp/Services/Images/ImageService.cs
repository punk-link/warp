﻿using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Data;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;

namespace Warp.WebApp.Services.Images;

public class ImageService : IImageService
{
    public ImageService(IDataStorage dataStorage)
    {
        _dataStorage = dataStorage;
    }


    public async Task<Dictionary<string, Guid>> Add(List<IFormFile> files, CancellationToken cancellationToken)
    {
        // TODO: add validation and a count check

        var results = new Dictionary<string, Guid>(files.Count);
        foreach (var file in files)
        {
            var (clientFileName, id) = await Add(file, cancellationToken);
            results.Add(clientFileName, id);
        }

        return results;
    }


    public async Task Attach(Guid entryId, TimeSpan relativeExpirationTime, List<Guid> imageIds, CancellationToken cancellationToken)
    {
        if (imageIds.Count == 0)
            return;

        var imageInfos = new List<ImageInfo>(imageIds.Count);
        foreach (var imageId in imageIds)
        {
            var entryCacheKey = BuildEntryCacheKey(imageId);
            var value = await _dataStorage.TryGet<ImageInfo>(entryCacheKey, cancellationToken);
            if (!value.Equals(default))
                imageInfos.Add(value);
        }

        var bucketCacheKey = BuildBucketCacheKey(entryId);
        await _dataStorage.Set(bucketCacheKey, imageInfos, relativeExpirationTime, cancellationToken);

        foreach (var imageInfo in imageInfos)
        {
            var entryCacheKey = BuildEntryCacheKey(imageInfo.Id);
            await _dataStorage.Remove<List<ImageInfo>>(entryCacheKey, cancellationToken);
        }
    }


    public async Task<List<ImageInfo>> Get(Guid entryId, CancellationToken cancellationToken)
    {
        var bucketCacheKey = BuildBucketCacheKey(entryId);
        var values = await _dataStorage.TryGet<List<ImageInfo>>(bucketCacheKey, cancellationToken);

        return values ?? Enumerable.Empty<ImageInfo>().ToList();
    }


    public async Task<Result<ImageInfo, ProblemDetails>> Get(Guid entryId, Guid imageId, CancellationToken cancellationToken)
    {
        var images = await Get(entryId, cancellationToken);
        var image = images.FirstOrDefault(x => x.Id == imageId);

        return image != default
            ? Result.Success<ImageInfo, ProblemDetails>(image)
            : ResultHelper.NotFound<ImageInfo>();
    }


    private static string BuildBucketCacheKey(Guid id)
        => $"{nameof(List<ImageInfo>)}::{id}";


    private static string BuildEntryCacheKey(Guid id)
        => $"{nameof(ImageInfo)}::{id}";


    private async Task<(string, Guid)> Add(IFormFile file, CancellationToken cancellationToken)
    {
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);

        var imageInfo = new ImageInfo
        {
            Id = Guid.NewGuid(),
            Content = memoryStream.ToArray(),
            ContentType = file.ContentType
        };

        var cacheKey = BuildEntryCacheKey(imageInfo.Id);
        await _dataStorage.Set(cacheKey, imageInfo, TimeSpan.FromHours(1), cancellationToken);

        return (file.FileName, imageInfo.Id);
    }


    private readonly IDataStorage _dataStorage;
}