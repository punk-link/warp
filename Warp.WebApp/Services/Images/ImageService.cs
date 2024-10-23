using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Warp.WebApp.Data.S3;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;

namespace Warp.WebApp.Services.Images;

public class ImageService : IImageService
{
    public ImageService(IStringLocalizer<ServerResources> localizer, IS3FileStorage s3FileStorage)
    {
        _localizer = localizer;

        _s3FileStorage = s3FileStorage;
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


    public async Task<List<Guid>> Attach(List<Guid> imageIds, CancellationToken cancellationToken)
    {
        if (imageIds.Count == 0)
            return Enumerable.Empty<Guid>().ToList();

        // TODO: try to use a Redis set instead of a list
        var imageInfos = new List<ImageInfo>(imageIds.Count);
        foreach (var imageId in imageIds)
        {
            var value = await GetImage(imageId, cancellationToken);

            if (!value.Equals(default))
                imageInfos.Add(value);
        }

        return imageInfos.Select(x => x.Id).ToList();
    }


    public async Task<List<ImageInfo>> GetImageList(List<Guid> imageIds, CancellationToken cancellationToken)
    {
        var values = new List<ImageInfo>();
        foreach(var imageId in imageIds)
        {
            var value = await GetImage(imageId, cancellationToken);

            if(value != default)
                values.Add(value);
        }

        return values;
    }


    private async Task<ImageInfo> GetImage(Guid imageId, CancellationToken cancellationToken)
        => await _s3FileStorage.Get(imageId, cancellationToken);


    public async Task<Result<ImageInfo, ProblemDetails>> Get(Guid imageId, CancellationToken cancellationToken)
    {
        var image = await GetImage(imageId, cancellationToken);

        return image != default
            ? Result.Success<ImageInfo, ProblemDetails>(image)
            : ResultHelper.NotFound<ImageInfo>(_localizer);
    }


    public async Task Remove(Guid imageId, CancellationToken cancellationToken)
        => await _s3FileStorage.Delete(imageId, cancellationToken);


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

        await _s3FileStorage.Save(imageInfo, cancellationToken);

        return (file.FileName, imageInfo.Id);
    }


    // TODO: make URL generation more flexible
    public static List<string> BuildImageUrls(Guid id, List<Guid> imageIds)
    => imageIds.Select(imageId => $"/api/images/entry-id/{IdCoder.Encode(id)}/image-id/{IdCoder.Encode(imageId)}")
        .ToList();


    private readonly IStringLocalizer<ServerResources> _localizer;
    private readonly IS3FileStorage _s3FileStorage;
}