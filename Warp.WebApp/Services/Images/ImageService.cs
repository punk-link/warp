using CSharpFunctionalExtensions;
using CSharpFunctionalExtensions.ValueTasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Diagnostics;
using Warp.WebApp.Data.S3;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Files;

namespace Warp.WebApp.Services.Images;

public class ImageService : IImageService, IUnauthorizedImageService
{
    public ImageService(IStringLocalizer<ServerResources> localizer, IS3FileStorage s3FileStorage)
    {
        _localizer = localizer;
        _s3FileStorage = s3FileStorage;
    }


    public async Task<Result<ImageResponse, ProblemDetails>> Add(Guid entryId, AppFile appFile, CancellationToken cancellationToken)
    {
        Debug.Assert(appFile.Content is not null, "A file content is null, because we checked it already at the controller level.");
        
        return await UnitResult.Success<ProblemDetails>()
            .Map(() => appFile)
            .Ensure(IsImageMimeType, ProblemDetailsHelper.Create(_localizer["Unsupported file extension."]))
            .Bind(Upload)
            .Bind(BuildImageInfo);


        bool IsImageMimeType(AppFile fileContent) 
            => _imageMimeTypes.Contains(fileContent.ContentMimeType);


        async Task<Result<(Guid, AppFile), ProblemDetails>> Upload(AppFile appFile)
        {
            var imageId = Guid.NewGuid();

            var result = await _s3FileStorage.Save(entryId.ToString(), imageId.ToString(), appFile, cancellationToken);
            if (result.IsFailure)
                return result.Error;

            return (imageId, appFile);
        }


        Result<ImageResponse, ProblemDetails> BuildImageInfo((Guid ImageId, AppFile AppFile) tuple)
        {
            var encodedEntryId = IdCoder.Encode(entryId);

            var url = BuildUrl(encodedEntryId, tuple.ImageId);
            var imageInfo = new ImageInfo(tuple.ImageId, entryId, url);
            
            return new ImageResponse(imageInfo, tuple.AppFile.UntrustedFileName);
        }
    }


    public Task<Result<Image, ProblemDetails>> Get(Guid entryId, Guid imageId, CancellationToken cancellationToken)
    {
        return GetImage(imageId, cancellationToken)
            .Bind(BuildImage);


        Task<Result<AppFile, ProblemDetails>> GetImage(Guid imageId, CancellationToken cancellationToken) 
            => _s3FileStorage.Get(entryId.ToString(), imageId.ToString(), cancellationToken);


        Result<Image, ProblemDetails> BuildImage(AppFile stream)
            => new Image()
            {
                Id = imageId,
                Content = stream.Content,
                ContentType = stream.ContentMimeType
            };
    }


    public Task<Result<List<ImageInfo>, ProblemDetails>> GetAttached(Guid entryId, List<Guid> imageIds, CancellationToken cancellationToken)
    { 
        return GetUploaded()
            .Bind(BuildImageInfo);


        async Task<Result<List<Guid>, ProblemDetails>> GetUploaded()
        {
            var prefix = entryId.ToString();
            var keys = imageIds.Select(imageId => imageId.ToString())
                .ToList();
            
            var (_, isFailure, stringKeys, error) = await _s3FileStorage.Contains(prefix, keys, cancellationToken);
            if (isFailure)
                return error;

            return stringKeys
                .Select(key => Guid. TryParse(key, out var imageId) ? imageId : Guid.Empty)
                .Where(imageId => imageId != Guid.Empty)
                .ToList();
        }


        Result<List<ImageInfo>, ProblemDetails> BuildImageInfo(List<Guid> imageIds) 
            => imageIds.Select(imageId =>
                {
                    var url = BuildUrl(entryId, imageId);
                    return new ImageInfo(imageId, entryId, url);
                }).ToList();
    }


    public Task<UnitResult<ProblemDetails>> Remove(Guid entryId, Guid imageId, CancellationToken cancellationToken) 
        => _s3FileStorage.Delete(entryId.ToString(), imageId.ToString(), cancellationToken);


    private static Uri BuildUrl(string encodedEntryId, Guid imageId)
        => new(string.Format("/api/images/entry-id/{0}/image-id/{1}", encodedEntryId, IdCoder.Encode(imageId)), UriKind.Relative);


    private static Uri BuildUrl(Guid entryId, Guid imageId)
        => BuildUrl(IdCoder.Encode(entryId), imageId);


    private static readonly HashSet<string> _imageMimeTypes = new(
    [
        "image/bmp",
        "image/gif",
        "image/jpeg",
        "image/jpeg",
        "image/png",
        "image/svg+xml",
        "image/tiff",
        "image/tiff",
        "image/webp",
        "image/x-icon"
    ], StringComparer.OrdinalIgnoreCase);


    private readonly IStringLocalizer<ServerResources> _localizer;
    private readonly IS3FileStorage _s3FileStorage;
}