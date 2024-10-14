using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Warp.WebApp.Data;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;

namespace Warp.WebApp.Services.Images;

public class ImageService : IImageService
{
    public ImageService(IDataStorage dataStorage, IStringLocalizer<ServerResources> localizer)
    {
        _dataStorage = dataStorage;
        _localizer = localizer;
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


    public async Task<List<Guid>> Attach(Guid entryId, TimeSpan relativeExpirationTime, List<Guid> imageIds, CancellationToken cancellationToken)
    {
        if (imageIds.Count == 0)
            return Enumerable.Empty<Guid>().ToList();

        // TODO: try to use a Redis set instead of a list
        var imageInfos = new List<ImageInfo>(imageIds.Count);
        foreach (var imageId in imageIds)
        {
            var entryCacheKey = CacheKeyBuilder.BuildImageInfoCacheKey(imageId);
            var value = await _dataStorage.TryGet<ImageInfo>(entryCacheKey, cancellationToken);
            if (!value.Equals(default))
                imageInfos.Add(value);
        }

        var bucketCacheKey = CacheKeyBuilder.BuildImageInfoListCacheKey(entryId);
        await _dataStorage.Set(bucketCacheKey, imageInfos, relativeExpirationTime, cancellationToken);

        foreach (var imageInfo in imageInfos)
        {
            var entryCacheKey = CacheKeyBuilder.BuildImageInfoCacheKey(imageInfo.Id);
            await _dataStorage.Remove<List<ImageInfo>>(entryCacheKey, cancellationToken);
        }

        return imageInfos.Select(x => x.Id).ToList();
    }


    public async Task<List<ImageInfo>> Get(Guid entryId, CancellationToken cancellationToken)
    {
        var bucketCacheKey = CacheKeyBuilder.BuildImageInfoListCacheKey(entryId);
        var values = await _dataStorage.TryGet<List<ImageInfo>>(bucketCacheKey, cancellationToken);

        return values ?? Enumerable.Empty<ImageInfo>().ToList();
    }


    public async Task<Result<ImageInfo, ProblemDetails>> Get(Guid entryId, Guid imageId, CancellationToken cancellationToken)
    {
        var images = await Get(entryId, cancellationToken);
        var image = images.FirstOrDefault(x => x.Id == imageId);

        return image != default
            ? Result.Success<ImageInfo, ProblemDetails>(image)
            : ResultHelper.NotFound<ImageInfo>(_localizer);
    }


    private async Task<(string, Guid)> Add(IFormFile file, CancellationToken cancellationToken)
    {
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);

        var imageInfo = new ImageInfo
        {
            Id = Guid.NewGuid(),
            Content = memoryStream.ToArray(),
            ContentType = file.ContentType
        };

        var cacheKey = CacheKeyBuilder.BuildImageInfoCacheKey(imageInfo.Id);
        await _dataStorage.Set(cacheKey, imageInfo, TimeSpan.FromHours(1), cancellationToken);

        return (file.FileName, imageInfo.Id);
    }


    public static List<string> BuildImageUrls(Guid id, List<Guid> imageIds)
    => imageIds.Select(imageId => $"/api/images/entry-id/{IdCoder.Encode(id)}/image-id/{IdCoder.Encode(imageId)}")
        .ToList();


    private readonly IDataStorage _dataStorage;
    private readonly IStringLocalizer<ServerResources> _localizer;
}