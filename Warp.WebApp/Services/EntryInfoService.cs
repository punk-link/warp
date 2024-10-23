using CSharpFunctionalExtensions;
using CSharpFunctionalExtensions.ValueTasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Warp.WebApp.Attributes;
using Warp.WebApp.Data;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Creators;
using Warp.WebApp.Services.Creators;
using Warp.WebApp.Services.Entries;
using Warp.WebApp.Services.Images;
using Warp.WebApp.Telemetry.Logging;

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
        return AddEntry()
            .Bind(BuildEntryInfo)
            // TODO: bound with a transaction
            .Bind(AttachToCreator)
            .Tap(CacheEntryInfo);


        Task<Result<Entry, ProblemDetails>> AddEntry()
            => _entryService.Add(entryRequest, cancellationToken);


        Result<EntryInfo, ProblemDetails> BuildEntryInfo(Entry entry)
        {
            var description = _openGraphService.BuildDescription(entry);
            return new EntryInfo(creator.Id, entry, description);
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
            var cacheKey = CacheKeyBuilder.BuildEntryInfoCacheKey(entryInfo.Entry.Id);
            return _dataStorage.Set(cacheKey, entryInfo, entryRequest.ExpiresIn, cancellationToken);
        }
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
    public async Task<UnitResult<ProblemDetails>> RemoveImage(Creator creator, Guid entryId, Guid imageId, CancellationToken cancellationToken)
    {
        return await GetEntryInfo(entryId, cancellationToken)
            .Ensure(entryInfo => IsBelongsToCreator(entryInfo, creator), ProblemDetailsHelper.Create(_localizer["NoPermissionErrorMessage"]))
            // Remove image from the entry 
            .Tap(UpdateEntryInfo)
            .Tap(RemoveImage)
            .TapError(LogError);


        Task RemoveImage(EntryInfo entry)
            => _imageService.Remove(imageId, cancellationToken);


        async Task<Result<EntryInfo, ProblemDetails>> UpdateEntryInfo(EntryInfo entryInfo)
        {
            var expiresAt = entryInfo.Entry.ExpiresAt - DateTime.UtcNow;
            var cacheKey = CacheKeyBuilder.BuildEntryInfoCacheKey(entryInfo.Entry.Id);
            await _dataStorage.Set(cacheKey, entryInfo, expiresAt, cancellationToken);

            return entryInfo;
        }


        void LogError(ProblemDetails error)
            => _logger.LogImageRemovalError(imageId, error.Detail!);
    }


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
