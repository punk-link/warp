using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using Warp.WebApp.Data.S3;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Files;

namespace Warp.WebApp.Services.Images;

public class ImageService : IImageService, IUnauthorizedImageService
{
    public ImageService(IS3FileStorage s3FileStorage)
    {
        _s3FileStorage = s3FileStorage;
    }


    public async Task<List<ImageResponse>> Add(Guid entryId, List<IFormFile> files, CancellationToken cancellationToken)
    {
        // TODO: add validation and a count check

        var encodedEntryId = IdCoder.Encode(entryId);

        var results = new ConcurrentBag<ImageResponse>();
        await Parallel.ForEachAsync(files, cancellationToken, async (file, ctx) =>
        {
            await Upload(file, ctx)
                .Bind(BuildImageInfo)
                .Bind(BuildImageResponse)
                .Tap(AddToResults);
        });

        return [.. results];


        async Task<Result<(Guid, IFormFile), ProblemDetails>> Upload(IFormFile file, CancellationToken cancellationToken)
        {
            using var contentStream = new MemoryStream();
            await file.CopyToAsync(contentStream, cancellationToken);

            var imageId = Guid.NewGuid();
            var fileContent = new FileContent(contentStream, file.ContentType);

            var result = await _s3FileStorage.Save(entryId.ToString(), imageId.ToString(), fileContent, cancellationToken);
            if (result.IsFailure)
                return result.Error;

            return (imageId, file);
        }


        Result<(ImageInfo, IFormFile), ProblemDetails> BuildImageInfo((Guid ImageId, IFormFile File) tuple)
        {
            var url = BuildUrl(encodedEntryId, tuple.ImageId);
            var imageInfo = new ImageInfo(tuple.ImageId, entryId, url);
            
            return (imageInfo, tuple.File);
        }


        Result<ImageResponse, ProblemDetails> BuildImageResponse((ImageInfo ImageInfo, IFormFile File) tuple) 
            => new ImageResponse
            {
                ImageInfo = tuple.ImageInfo,
                ClientFileName = tuple.File.FileName
            };


        void AddToResults(ImageResponse imageInfo) 
            => results.Add(imageInfo);
    }


    public Task<Result<Image, ProblemDetails>> Get(Guid entryId, Guid imageId, CancellationToken cancellationToken)
    {
        return GetImage(imageId, cancellationToken)
            .Bind(BuildImage);


        Task<Result<FileContent, ProblemDetails>> GetImage(Guid imageId, CancellationToken cancellationToken) 
            => _s3FileStorage.Get(entryId.ToString(), imageId.ToString(), cancellationToken);


        Result<Image, ProblemDetails> BuildImage(FileContent stream)
            => new Image()
            {
                Id = imageId,
                Content = stream.Content,
                ContentType = stream.ContentType
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


    private readonly IS3FileStorage _s3FileStorage;
}