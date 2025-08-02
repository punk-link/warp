using CSharpFunctionalExtensions;
using CSharpFunctionalExtensions.ValueTasks;
using System.Diagnostics;
using System.IO.Hashing;
using Warp.WebApp.Constants.Caching;
using Warp.WebApp.Data;
using Warp.WebApp.Data.S3;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Models.Files;
using Warp.WebApp.Models.Images;

namespace Warp.WebApp.Services.Images;

/// <summary>
/// Service responsible for managing images in the application.
/// Implements both <see cref="IImageService"/> for authorized image operations and 
/// <see cref="IUnauthorizedImageService"/> for operations that don't require authorization.
/// </summary>
public class ImageService : IImageService, IUnauthorizedImageService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageService"/> class.
    /// </summary>
    /// <param name="dataStorage">The data storage provider for caching operations.</param>
    /// <param name="s3FileStorage">The S3 file storage provider for image persistence.</param>
    public ImageService(IDataStorage dataStorage, IS3FileStorage s3FileStorage)
    {
        _dataStorage = dataStorage;
        _s3FileStorage = s3FileStorage;
    }


    /// <inheritdoc cref="IUnauthorizedImageService.Add"/>
    public async Task<Result<ImageInfo, DomainError>> Add(Guid entryId, AppFile appFile, CancellationToken cancellationToken)
    {
        Debug.Assert(appFile.Content is not null, "A file content is null, because we checked it already at the controller level.");
        
        return await UnitResult.Success<DomainError>()
            .Map(() => appFile)
            .Ensure(IsImageMimeType, DomainErrors.UnsupportedFileExtension(appFile.ContentMimeType, string.Join(", ", _imageMimeTypes)))
            .Bind(CheckDuplicate)
            .Bind(Upload)
            .Bind(AddHash)
            .Bind(BuildImageInfo);


        bool IsImageMimeType(AppFile fileContent) 
            => _imageMimeTypes.Contains(fileContent.ContentMimeType);


        async Task<Result<AppFile, DomainError>> CheckDuplicate(AppFile file)
        {
            var hash = await CalculateFileHash(file.Content, cancellationToken);
            if (!await IsHashCached(hash))
                return AppFile.AddHash(file, hash);

            return DomainErrors.ImageAlreadyExists(file.UntrustedFileName);
            

            static async Task<string> CalculateFileHash(Stream stream, CancellationToken ct)
            {
                stream.Position = 0;
        
                var hash = new XxHash128();
                byte[] buffer = new byte[81920]; // 80KB buffer
                int bytesRead;
        
                while ((bytesRead = await stream.ReadAsync(buffer, ct)) > 0)
                    hash.Append(buffer.AsSpan(0, bytesRead));
        
                Span<byte> hashBytes = stackalloc byte[hash.HashLengthInBytes];
                hash.GetCurrentHash(hashBytes);
        
                stream.Position = 0;
                return Convert.ToHexString(hashBytes);
            }


            async ValueTask<bool> IsHashCached(string hash)
            {
                var cacheKey = CacheKeyBuilder.BuildImageHashCacheKey(entryId, hash);
                return await _dataStorage.Contains<bool>(cacheKey, cancellationToken);
            }
        }


        async Task<Result<(Guid, AppFile), DomainError>> Upload(AppFile appFile)
        {
            var imageId = Guid.NewGuid();

            var result = await _s3FileStorage.Save(entryId.ToString(), imageId.ToString(), appFile, cancellationToken);
            if (result.IsFailure)
                return result.Error;

            return (imageId, appFile);
        }


        async Task<Result<(Guid, AppFile), DomainError>> AddHash((Guid ImageId, AppFile AppFile) tuple)
        {
            var imageHashCacheKey = CacheKeyBuilder.BuildImageToHashBindingCacheKey(tuple.ImageId);
            await _dataStorage.Set(imageHashCacheKey, tuple.AppFile.Hash, CachingConstants.MaxSupportedCachingTime, cancellationToken);

            var cacheKey = CacheKeyBuilder.BuildImageHashCacheKey(entryId, tuple.AppFile.Hash);
            await _dataStorage.Set(cacheKey, true, CachingConstants.MaxSupportedCachingTime, cancellationToken);

            return tuple;
        }


        Result<ImageInfo, DomainError> BuildImageInfo((Guid ImageId, AppFile AppFile) tuple)
        {
            var url = BuildUrl(in entryId, in tuple.ImageId);
            return new ImageInfo(tuple.ImageId, entryId, url);
        }
    }


    /// <inheritdoc cref="IUnauthorizedImageService.BuildPartialUrl"/>
    public Uri BuildPartialUrl(in Guid entryId, in Guid imageId) 
        => new(string.Format(ImageUrlBaseTemplate + "/partial", IdCoder.Encode(entryId), IdCoder.Encode(imageId)), UriKind.Relative);


    /// <inheritdoc cref="IUnauthorizedImageService.BuildUrl"/>
    public Uri BuildUrl(in Guid entryId, in Guid imageId)
        => new(string.Format(ImageUrlBaseTemplate, IdCoder.Encode(entryId), IdCoder.Encode(imageId)), UriKind.Relative);


    /// <inheritdoc cref="IImageService.Copy"/>
    public async Task<Result<List<ImageInfo>, DomainError>> Copy(Guid sourceEntryId, Guid targetEntryId, List<ImageInfo> sourceImages, CancellationToken cancellationToken)
    {
        if (sourceImages.Count == 0)
            return Enumerable.Empty<ImageInfo>().ToList();

        var results = new List<ImageInfo>();
        foreach (var sourceImage in sourceImages)
        {
            var result = await Get(sourceEntryId, sourceImage.Id, cancellationToken)
                .Map(CreateAppFile)
                .Bind(CopyToTarget)
                .Tap(results.Add);

            if (result.IsFailure)
                return result.Error;
        }
        
        return results;


        static AppFile CreateAppFile(Image image) 
            => new(image.Content, image.ContentType);


        Task<Result<ImageInfo, DomainError>> CopyToTarget(AppFile appFile) 
            => Add(targetEntryId, appFile, cancellationToken);
    }


    /// <inheritdoc cref="IUnauthorizedImageService.Get"/>
    public Task<Result<Image, DomainError>> Get(Guid entryId, Guid imageId, CancellationToken cancellationToken)
    {
        return GetImage(imageId, cancellationToken)
            .Bind(BuildImage);


        Task<Result<AppFile, DomainError>> GetImage(Guid imageId, CancellationToken cancellationToken) 
            => _s3FileStorage.Get(entryId.ToString(), imageId.ToString(), cancellationToken);


        Result<Image, DomainError> BuildImage(AppFile stream)
            => new Image()
            {
                Id = imageId,
                Content = stream.Content,
                ContentType = stream.ContentMimeType
            };
    }


    /// <inheritdoc cref="IImageService.GetAttached"/>
    public Task<Result<List<ImageInfo>, DomainError>> GetAttached(Guid entryId, List<Guid> imageIds, CancellationToken cancellationToken)
    { 
        return GetUploaded()
            .Bind(BuildImageInfo);


        async Task<Result<List<Guid>, DomainError>> GetUploaded()
        {
            var prefix = entryId.ToString();
            var keys = imageIds
                .Where(imageId => imageId != Guid.Empty)
                .Select(imageId => imageId.ToString())
                .ToList();
            
            var (_, isFailure, stringKeys, error) = await _s3FileStorage.Contains(prefix, keys, cancellationToken);
            if (isFailure)
                return error;

            return stringKeys
                .Select(key => Guid. TryParse(key, out var imageId) ? imageId : Guid.Empty)
                .Where(imageId => imageId != Guid.Empty)
                .ToList();
        }


        Result<List<ImageInfo>, DomainError> BuildImageInfo(List<Guid> imageIds) 
            => imageIds.Select(imageId =>
                {
                    var url = BuildUrl(entryId, imageId);
                    return new ImageInfo(imageId, entryId, url);
                }).ToList();
    }


    /// <inheritdoc cref="IImageService.Remove"/>
    public Task<UnitResult<DomainError>> Remove(Guid entryId, Guid imageId, CancellationToken cancellationToken)
    {
        return GetImageHash()
            .Tap(CleanupHash)
            .Bind(DeleteFile);


        async Task<Result<string?, DomainError>> GetImageHash()
        {
            var imageHashCacheKey = CacheKeyBuilder.BuildImageToHashBindingCacheKey(imageId);
            return await _dataStorage.TryGet<string>(imageHashCacheKey, cancellationToken);
        }

    
        async Task CleanupHash(string? hash)
        {
            if (hash is null)
                return;

            var imageHashCacheKey = CacheKeyBuilder.BuildImageToHashBindingCacheKey(imageId);
            var hashCacheKey = CacheKeyBuilder.BuildImageHashCacheKey(entryId, hash);
            
            await _dataStorage.Remove<string>(hashCacheKey, cancellationToken);
            await _dataStorage.Remove<string>(imageHashCacheKey, cancellationToken);
        }
        
    
        Task<UnitResult<DomainError>> DeleteFile(string? _)
            => _s3FileStorage.Delete(entryId.ToString(), imageId.ToString(), cancellationToken);
    }
    
    
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

    private const string ImageUrlBaseTemplate = "/api/images/entry-id/{0}/image-id/{1}";


    private readonly IDataStorage _dataStorage;
    private readonly IS3FileStorage _s3FileStorage;
}