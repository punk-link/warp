using CSharpFunctionalExtensions;
using CSharpFunctionalExtensions.ValueTasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Threading;
using Warp.WebApp.Attributes;
using Warp.WebApp.Data;
using Warp.WebApp.Extensions;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Creators;
using Warp.WebApp.Models.Validators;
using Warp.WebApp.Services.Creators;
using Warp.WebApp.Services.Entries;
using Warp.WebApp.Services.Images;
using Warp.WebApp.Services.OpenGraph;
using Warp.WebApp.Telemetry.Logging;
using static System.Net.Mime.MediaTypeNames;

namespace Warp.WebApp.Services;

public class EntryInfoService : IEntryInfoService
{
    public EntryInfoService(ICreatorService creatorService, 
        IDataStorage dataStorage,
        IEntryService entryService,
        IImageService imageService,
        ILoggerFactory loggerFactory,
        IOpenGraphService openGraphService,
        IReportService reportService,
        IStringLocalizer<ServerResources> localizer,
        IViewCountService viewCountService)
    {
        _logger = loggerFactory.CreateLogger<EntryInfoService>();

        _creatorService = creatorService;
        _dataStorage = dataStorage;
        _entryService = entryService;
        _imageService = imageService;
        _localizer = localizer;
        _openGraphService = openGraphService;
        _reportService = reportService;
        _viewCountService = viewCountService;
    }


    [TraceMethod]
    public Task<Result<EntryInfo, ProblemDetails>> Add(Creator creator, EntryRequest entryRequest, CancellationToken cancellationToken)
    {
        var entryInfoId = entryRequest.Id;
        var now = DateTime.UtcNow;

        return AddEntry()
            .Bind(GetImageInfos)
            .Bind(BuildEntryInfo)
            .Bind(Validate)
            .Bind(AttachToCreator)
            .Tap(CacheEntryInfo);


        Task<Result<Entry, ProblemDetails>> AddEntry()
            => _entryService.Add(entryRequest, cancellationToken);


        async Task<Result<(Entry, List<ImageInfo>), ProblemDetails>> GetImageInfos(Entry entry)
        {
            var (_, isFailure, imageInfos, error) = await _imageService.GetAttached(entryInfoId, entryRequest.ImageIds, cancellationToken);
            if (isFailure)
                return error;

            return (entry, imageInfos);
        }


        Result<EntryInfo, ProblemDetails> BuildEntryInfo((Entry Entry, List<ImageInfo> ImageInfos) tuple)
        {
            var expirationTime = now + entryRequest.ExpiresIn;

            var previewImageUri = tuple.ImageInfos
                .Select(imageInfo => imageInfo.Url)
                .FirstOrDefault();
        
            var description = _openGraphService.BuildDescription(tuple.Entry.Content, previewImageUri);
            
            return new EntryInfo(entryInfoId, creator.Id, now, expirationTime, entryRequest.EditMode, tuple.Entry, tuple.ImageInfos, description, 0);
        }


        async Task<Result<EntryInfo, ProblemDetails>> Validate(EntryInfo entryInfo)
        {
            var validator = new EntryInfoValidator(_localizer);
            var validationResult = await validator.ValidateAsync(entryInfo, cancellationToken);
            if (!validationResult.IsValid)
                return validationResult.ToFailure<EntryInfo>(_localizer);

            return entryInfo;
        }


        async Task<Result<EntryInfo, ProblemDetails>> AttachToCreator(EntryInfo entryInfo)
        {
            var attachResult = await _creatorService.AttachEntry(creator, entryInfo, cancellationToken);
            if (attachResult.IsFailure)
                return attachResult.Error;

            return entryInfo;
        }


        Task CacheEntryInfo(EntryInfo entryInfo)
        {
            var cacheKey = CacheKeyBuilder.BuildEntryInfoCacheKey(entryInfo.Id);
            return _dataStorage.Set(cacheKey, entryInfo, entryRequest.ExpiresIn, cancellationToken);
        }
    }


    [TraceMethod]
    public Task<Result<EntryInfo, ProblemDetails>> Copy(Creator creator, Guid entryId, CancellationToken cancellationToken)
    {
        return GetEntryInfo(entryId, cancellationToken)
            .Ensure(entryInfo => IsBelongsToCreator(entryInfo, creator), ProblemDetailsHelper.Create(_localizer["NoPermissionErrorMessage"]))
            .Map(BuildEntryRequest)
            .Bind(AddEntryInfo);


        static EntryRequest BuildEntryRequest(EntryInfo entryInfo)
        {
            var expiresIn = entryInfo.ExpiresAt - entryInfo.CreatedAt;
            return new EntryRequest
            {
                TextContent = entryInfo.Entry.Content,
                ExpiresIn = expiresIn,
                ImageIds = [] // TODO: add image ids when the feature is implemented
            };
        }


        Task<Result<EntryInfo, ProblemDetails>> AddEntryInfo(EntryRequest entryRequest)
            => Add(creator, entryRequest, cancellationToken);
    }


    [TraceMethod]
    public async Task<Result<EntryInfo, ProblemDetails>> Get(Creator creator, Guid entryId, CancellationToken cancellationToken)
    {
        var result = await EnsureNotReported();
        if (result.IsFailure)
            return result.Error;

        return await GetEntryInfo(entryId, cancellationToken)
            .Bind(GetViews);


        async Task<UnitResult<ProblemDetails>> EnsureNotReported()
        {
            if (await _reportService.Contains(entryId, cancellationToken))
                return UnitResult.Failure(ProblemDetailsHelper.CreateNotFound(_localizer));

            return UnitResult.Success<ProblemDetails>();
        }


        async Task<Result<EntryInfo, ProblemDetails>> GetViews(EntryInfo entryInfo)
        {
            var viewCount = entryInfo.CreatorId == creator.Id
                ? await _viewCountService.Get(entryId, cancellationToken)
                : await _viewCountService.AddAndGet(entryId, cancellationToken);

            return entryInfo with { ViewCount = viewCount };
        }
    }


    [TraceMethod]
    public async Task<UnitResult<ProblemDetails>> Remove(Creator creator, Guid entryId, CancellationToken cancellationToken)
    {
        return await GetEntryInfo(entryId, cancellationToken)
            .Ensure(entryInfo => IsBelongsToCreator(entryInfo, creator), ProblemDetailsHelper.Create(_localizer["NoPermissionErrorMessage"]))
            .Tap(RemoveEntryInfo);


        Task RemoveEntryInfo()
        {
            var cacheKey = CacheKeyBuilder.BuildEntryInfoCacheKey(entryId);
            return _dataStorage.Remove<EntryInfo>(cacheKey, cancellationToken);
        }
    }


    [TraceMethod]
    public Task<UnitResult<ProblemDetails>> RemoveImage(Creator creator, Guid entryId, Guid imageId, CancellationToken cancellationToken) 
        => GetEntryInfo(entryId, cancellationToken)
            .Finally(result =>
            {
                if (result.IsFailure)
                    return RemoveUnattachedImageInternally(entryId, imageId, cancellationToken);

                return RemoveAttachedImageInternally(result, creator, entryId, imageId, cancellationToken);
            });


    private async Task<Result<EntryInfo, ProblemDetails>> GetEntryInfo(Guid entryId, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeyBuilder.BuildEntryInfoCacheKey(entryId);
        var result = await _dataStorage.TryGet<EntryInfo?>(cacheKey, cancellationToken);
        if (result is null)
            return ProblemDetailsHelper.CreateNotFound(_localizer);

        return result.Value;
    }


    private static bool IsBelongsToCreator(in EntryInfo entryInfo, in Creator creator)
        => entryInfo.CreatorId == creator.Id;


    private Task<UnitResult<ProblemDetails>> RemoveAttachedImageInternally(Result<EntryInfo, ProblemDetails> entryInfoResult, Creator creator, Guid entryId, Guid imageId, CancellationToken cancellationToken)
    {
        return entryInfoResult
            .Ensure(entryInfo => IsBelongsToCreator(entryInfo, creator), ProblemDetailsHelper.Create(_localizer["NoPermissionErrorMessage"]))
            .Bind(UpdateEntryInfo)
            .TapError(LogDomainError)
            .Tap(CacheEntryInfo)
            .Bind(RemoveImage);


        Result<EntryInfo, ProblemDetails> UpdateEntryInfo(EntryInfo entryInfo)
        { 
            entryInfo.ImageInfos.RemoveAll(imageInfo => imageInfo.Id == imageId);
            return entryInfo;
        }


        void LogDomainError(ProblemDetails error)
            => _logger.LogImageRemovalDomainError(imageId, error.Detail!);


        async Task CacheEntryInfo(EntryInfo entryInfo)
        {
            var expiresAt = entryInfo.ExpiresAt - DateTime.UtcNow;
            var cacheKey = CacheKeyBuilder.BuildEntryInfoCacheKey(entryInfo.Id);
            await _dataStorage.Set(cacheKey, entryInfo, expiresAt, cancellationToken);
        }


        Task<UnitResult<ProblemDetails>> RemoveImage(EntryInfo entry)
            => _imageService.Remove(entryId, imageId, cancellationToken);
    }


    private Task<UnitResult<ProblemDetails>> RemoveUnattachedImageInternally(Guid entryId, Guid imageId, CancellationToken cancellationToken) 
        => _imageService.Remove(entryId, imageId, cancellationToken);


    private readonly ICreatorService _creatorService;
    private readonly IDataStorage _dataStorage;
    private readonly IEntryService _entryService;
    private readonly IImageService _imageService;
    private readonly IStringLocalizer<ServerResources> _localizer;
    private readonly ILogger<EntryInfoService> _logger;
    private readonly IOpenGraphService _openGraphService;
    private readonly IReportService _reportService;
    private readonly IViewCountService _viewCountService;
}
