using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Warp.WebApp.Data;
using Warp.WebApp.Data.S3;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;

namespace Warp.WebApp.Services.Images;

public class ImageService : IImageService
{
    public ImageService(IDataStorage dataStorage, IStringLocalizer<ServerResources> localizer, IFileStorage fileStorage)
    {
        _dataStorage = dataStorage;
        _localizer = localizer;
        _fileStorage = fileStorage;
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
    {
        return await _fileStorage.GetFileFromStorage(imageId, cancellationToken);
    }


    public async Task<Result<ImageInfo, ProblemDetails>> Get(Guid imageId, CancellationToken cancellationToken)
    {
        var image = await GetImage(imageId, cancellationToken);

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

        await _fileStorage.SaveFileToStorage(imageInfo, cancellationToken);

        return (file.FileName, imageInfo.Id);
    }


    public static List<string> BuildImageUrls(Guid id, List<Guid> imageIds)
    => imageIds.Select(imageId => $"/api/images/entry-id/{id}/image-id/{imageId}")
        .ToList();

    private readonly IDataStorage _dataStorage;
    private readonly IStringLocalizer<ServerResources> _localizer;
    private readonly IFileStorage _fileStorage;
}