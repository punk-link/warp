using CSharpFunctionalExtensions;
using CSharpFunctionalExtensions.ValueTasks;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using Warp.WebApp.Attributes;
using Warp.WebApp.Data;
using Warp.WebApp.Models.Creators;
using Warp.WebApp.Models.Entries;
using Warp.WebApp.Models.Entries.Enums;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Models.Images;
using Warp.WebApp.Models.Validators;
using Warp.WebApp.Services.Creators;
using Warp.WebApp.Services.Entries;
using Warp.WebApp.Services.Images;
using Warp.WebApp.Services.OpenGraph;
using Warp.WebApp.Telemetry.Logging;
using Warp.WebApp.Telemetry.Metrics;

namespace Warp.WebApp.Services;

/// <summary>
/// Service responsible for managing entry information including creation, 
/// retrieval, copying, and removal of entries and their associated images.
/// </summary>
public partial class EntryInfoService : IEntryInfoService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntryInfoService"/> class.
    /// </summary>
    /// <param name="creatorService">The service for managing creators.</param>
    /// <param name="dataStorage">The data storage service for persisting entry information.</param>
    /// <param name="entryService">The service for managing entry content.</param>
    /// <param name="imageService">The service for managing images.</param>
    /// <param name="entryImageLifecycleService">The service for tracking entry image lifecycle metadata.</param>
    /// <param name="loggerFactory">The factory for creating loggers.</param>
    /// <param name="openGraphService">The service for generating OpenGraph descriptions.</param>
    /// <param name="reportService">The service for handling entry reports.</param>
    /// <param name="viewCountService">The service for tracking entry view counts.</param>
    /// <param name="entryInfoMetrics">The metrics recorder for entry info actions.</param>
    public EntryInfoService(ICreatorService creatorService, 
        IDataStorage dataStorage,
        IEntryService entryService,
        IImageService imageService,
        IEntryImageLifecycleService entryImageLifecycleService,
        ILoggerFactory loggerFactory,
        IOpenGraphService openGraphService,
        IReportService reportService,
        IViewCountService viewCountService,
        IEntryInfoMetrics entryInfoMetrics)
    {
        _logger = loggerFactory.CreateLogger<EntryInfoService>();

        _creatorService = creatorService;
        _dataStorage = dataStorage;
        _entryImageLifecycleService = entryImageLifecycleService;
        _entryService = entryService;
        _imageService = imageService;
        _openGraphService = openGraphService;
        _reportService = reportService;
        _viewCountService = viewCountService;
        _entryInfoMetrics = entryInfoMetrics;
    }


    /// <inheritdoc/>
    [TraceMethod]
    public async Task<Result<EntryInfo, DomainError>> Add(Creator creator, EntryRequest entryRequest, CancellationToken cancellationToken)
    {
        var entryInfoId = entryRequest.Id;
        var now = DateTimeOffset.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        return await AddEntry()
            .Bind(GetImageInfos)
            .Bind(AddOpenGraphDescription)
            .Bind(BuildEntryInfo)
            .Tap(RecordContentSizeMetric)
            .Bind(Validate)
            .Bind(AttachToCreator)
            .Tap(CacheEntryInfo)
            .Tap(TrackEntryLifecycle)
            .Finally(result =>
            {
                TrackResult(EntryInfoMetricActions.Add, stopwatch, result);
                return Task.FromResult(result);
            });


        Task<Result<Entry, DomainError>> AddEntry()
            => _entryService.Add(entryRequest, cancellationToken);


        async Task<Result<(Entry, List<ImageInfo>), DomainError>> GetImageInfos(Entry entry)
        {
            var (_, isFailure, imageInfos, error) = await _imageService.GetAttached(entryInfoId, entryRequest.ImageIds, cancellationToken);
            if (isFailure)
                return error;

            return (entry, imageInfos);
        }


        async Task<Result<(Entry, List<ImageInfo>), DomainError>> AddOpenGraphDescription((Entry Entry, List<ImageInfo> ImageInfos) tuple)
        {
            var previewImageUri = tuple.ImageInfos
                .Select(imageInfo => imageInfo.Url)
                .FirstOrDefault();

            var addResult = await _openGraphService.Add(entryInfoId, tuple.Entry.Content, previewImageUri, entryRequest.ExpiresIn, cancellationToken);
            if (addResult.IsFailure)
                return addResult.Error;

            return tuple;
        }


        Result<EntryInfo, DomainError> BuildEntryInfo((Entry Entry, List<ImageInfo> ImageInfos) tuple)
        {
            var expirationTime = now + entryRequest.ExpiresIn;
            return new EntryInfo(entryInfoId, creator.Id, now, expirationTime, entryRequest.EditMode, tuple.Entry, tuple.ImageInfos, 0);
        }


        void RecordContentSizeMetric(EntryInfo entryInfo)
        {
            var content = entryInfo.Entry.Content;
            var plainTextContent = entryInfo.EditMode == EditMode.Advanced 
                ? StripHtmlTags(content) 
                : content;

            var characterCount = plainTextContent.Length;
            var editMode = entryInfo.EditMode.ToString().ToLowerInvariant();

            var tags = new TagList
            {
                { EntryInfoMetricTagNames.EditMode, editMode }
            };

            ApplicationMetrics.EntryContentSizeChars.Record(characterCount, tags);
        }


        async Task<Result<EntryInfo, DomainError>> Validate(EntryInfo entryInfo)
        {
            var validator = new EntryInfoValidator();
            var validationResult = await validator.ValidateAsync(entryInfo, cancellationToken);
            if (validationResult.IsValid)
                return entryInfo;

            var error = DomainErrors.EntryInfoModelValidationError();
            foreach (var validationError in validationResult.Errors)
                error.WithExtension($"{nameof(EntryInfoValidator)}:{validationError.ErrorCode}", validationError.ErrorMessage);

            return error;
        }


        async Task<Result<EntryInfo, DomainError>> AttachToCreator(EntryInfo entryInfo)
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


        Task TrackEntryLifecycle(EntryInfo entryInfo)
        {
            var imageIds = entryInfo.ImageInfos.Select(imageInfo => imageInfo.Id);
            return _entryImageLifecycleService.Track(entryInfo.Id, entryInfo.ExpiresAt, imageIds, cancellationToken);
        }
    }


    /// <inheritdoc/>
    [TraceMethod]
    public async Task<Result<EntryInfo, DomainError>> Copy(Creator creator, Guid entryId, CancellationToken cancellationToken)
    {
        var newEntryId = Guid.CreateVersion7();
        var stopwatch = Stopwatch.StartNew();

        return await GetEntryInfo(entryId, cancellationToken)
            .Ensure(entryInfo => IsBelongsToCreator(entryInfo, creator), DomainErrors.NoPermissionError())
            .Bind(CopyOpenGraphDescription)
            .Bind(CopyImages)
            .Bind(BuildEntryRequest)
            .Bind(AddEntryInfo)
            .Finally(result => 
            { 
                TrackResult(EntryInfoMetricActions.Copy, stopwatch, result);
                return Task.FromResult(result);
            });


        async Task<Result<EntryInfo, DomainError>> CopyOpenGraphDescription(EntryInfo entryInfo)
        {
            var description = await _openGraphService.Get(entryId, cancellationToken);
            var expiresIn = entryInfo.ExpiresAt - entryInfo.CreatedAt;

            var addResult = await _openGraphService.Add(newEntryId, description, expiresIn, cancellationToken);
            if (addResult.IsFailure)
                return addResult.Error;

            return entryInfo;
        }


        async Task<Result<(EntryInfo, List<Guid>), DomainError>> CopyImages(EntryInfo entryInfo)
        {
            if (entryInfo.ImageInfos.Count == 0)
                return (entryInfo, []);

            var copyResult = await _imageService.Copy(entryInfo.Id, newEntryId, entryInfo.ImageInfos, cancellationToken);
            if (copyResult.IsFailure)
                return copyResult.Error;

            var newImageIds = copyResult.Value.Select(img => img.Id).ToList();
            return (entryInfo, newImageIds);
        }


        Result<EntryRequest, DomainError> BuildEntryRequest((EntryInfo EntryInfo, List<Guid> NewImageIds) tuple)
        {
            var expiresIn = tuple.EntryInfo.ExpiresAt - tuple.EntryInfo.CreatedAt;
            return new EntryRequest
            {
                Id = newEntryId,
                TextContent = tuple.EntryInfo.Entry.Content,
                ExpiresIn = expiresIn,
                EditMode = tuple.EntryInfo.EditMode,
                ImageIds = tuple.NewImageIds
            };
        }


        Task<Result<EntryInfo, DomainError>> AddEntryInfo(EntryRequest entryRequest)
            => Add(creator, entryRequest, cancellationToken);
    }


    /// <inheritdoc/>
    [TraceMethod]
    public async Task<Result<EntryInfo, DomainError>> Get(Creator creator, Guid entryId, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        return await EnsureNotReported(entryId, cancellationToken)
            .Bind(() => GetEntryInfo(entryId, cancellationToken))
            .Bind(GetOrAddViews)
            .Finally(result => 
            {
                TrackResult(EntryInfoMetricActions.Get, stopwatch, result);
                return Task.FromResult(result);
            });


        async Task<Result<EntryInfo, DomainError>> GetOrAddViews(EntryInfo entryInfo)
        {
            var viewCount = entryInfo.CreatorId == creator.Id
                ? await _viewCountService.Get(entryId, cancellationToken)
                : await _viewCountService.AddAndGet(entryId, cancellationToken);

            return entryInfo with { ViewCount = viewCount };
        }
    }


    /// <inheritdoc?>
    [TraceMethod]
    public async Task<Result<EntryOpenGraphDescription, DomainError>> GetOpenGraphDescription(Guid entryId, CancellationToken cancellationToken)
    { 
        var stopwatch = Stopwatch.StartNew();

        return await EnsureNotReported(entryId, cancellationToken)
            .Bind(GetDescription)
            .Finally(result => 
            {
                TrackResult(EntryInfoMetricActions.GetOpenGraphDescription, stopwatch, result);
                return Task.FromResult(result);
            });


        async Task<Result<EntryOpenGraphDescription, DomainError>> GetDescription() 
            => await _openGraphService.Get(entryId, cancellationToken);
    }


    /// <inheritdoc/>
    [TraceMethod]
    public async Task<UnitResult<DomainError>> IsEditable(Creator creator, Guid entryId, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        return await Evaluate()
            .Finally(result =>
            {
                TrackResult(EntryInfoMetricActions.IsEditable, stopwatch, result);
                return Task.FromResult(result);
            });


        async Task<UnitResult<DomainError>> Evaluate()
        {
            var entryInfoResult = await GetEntryInfo(entryId, cancellationToken);
            if (entryInfoResult.IsFailure)
                return entryInfoResult.Error;

            var entryInfo = entryInfoResult.Value;
            if (!IsBelongsToCreator(entryInfo, creator))
                return DomainErrors.NoPermissionError();

            var viewCount = await _viewCountService.Get(entryId, cancellationToken);
            if (viewCount != 0)
                return DomainErrors.EntryCannotBeEditedAfterViewed();

            return UnitResult.Success<DomainError>();
        }
    }


    /// <inheritdoc/>
    [TraceMethod]
    public async Task<UnitResult<DomainError>> Remove(Creator creator, Guid entryId, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        return await GetEntryInfo(entryId, cancellationToken)
            .Ensure(entryInfo => IsBelongsToCreator(entryInfo, creator), DomainErrors.NoPermissionError())
            .Tap(RemoveImages)
            .Tap(RemoveLifecycle)
            .Tap(RemoveEntryInfo)
            .Finally(result => 
            { 
                TrackResult(EntryInfoMetricActions.Remove, stopwatch, result);
                return Task.FromResult(result);
            });


        async Task RemoveImages(EntryInfo entryInfo)
        {
            foreach (var imageInfo in entryInfo.ImageInfos)
            {
                var removeResult = await _imageService.Remove(entryId, imageInfo.Id, cancellationToken);
                if (removeResult.IsFailure)
                    _logger.LogEntryImageCleanupFailure(imageInfo.Id, entryId, removeResult.Error.Detail);
            }
        }


        Task RemoveLifecycle(EntryInfo _)
            => _entryImageLifecycleService.Remove(entryId, cancellationToken);


        Task RemoveEntryInfo()
        {
            var cacheKey = CacheKeyBuilder.BuildEntryInfoCacheKey(entryId);
            return _dataStorage.Remove<EntryInfo>(cacheKey, cancellationToken);
        }
    }


    /// <inheritdoc/>
    [TraceMethod]
    public async Task<UnitResult<DomainError>> RemoveImage(Creator creator, Guid entryId, Guid imageId, CancellationToken cancellationToken) 
    {
        var stopwatch = Stopwatch.StartNew();

        return await GetEntryInfo(entryId, cancellationToken)
            .Finally(result =>
            {
                TrackResult(EntryInfoMetricActions.RemoveImage, stopwatch, result);

                return result.IsFailure
                    ? RemoveUnattachedImageInternally(entryId, imageId, cancellationToken)
                    : RemoveAttachedImageInternally(result, creator, entryId, imageId, cancellationToken);
            });
    }


    /// <inheritdoc/>
    [TraceMethod]
    public async Task<Result<EntryInfo, DomainError>> Update(Creator creator, EntryRequest entryRequest, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        return await GetEntryInfo(entryRequest.Id, cancellationToken)
            .Ensure(entryInfo => IsBelongsToCreator(entryInfo, creator), DomainErrors.NoPermissionError())
            .Ensure(entryInfo => entryInfo.EditMode == entryRequest.EditMode, DomainErrors.EntryEditModeMismatch())
            .Bind(GetViews)
            .Ensure(entryInfo => entryInfo.ViewCount == 0, DomainErrors.EntryCannotBeEditedAfterViewed())
            .Bind(_ => Add(creator, entryRequest, cancellationToken))
            .Finally(result => 
            {
                TrackResult(EntryInfoMetricActions.Update, stopwatch, result);
                return Task.FromResult(result);
            });


        async Task<Result<EntryInfo, DomainError>> GetViews(EntryInfo entryInfo)
        {
            var viewCount = await _viewCountService.Get(entryInfo.Id, cancellationToken);
            return entryInfo with { ViewCount = viewCount };
        }
    }


    private void TrackResult(string action, Stopwatch stopwatch, UnitResult<DomainError> result)
    {
        if (stopwatch.IsRunning)
            stopwatch.Stop();

        if (result.IsSuccess)
            TrackSuccess(action, stopwatch.Elapsed);
        else
            TrackFailure(action, stopwatch.Elapsed, result.Error);
    }


    private void TrackResult<T>(string action, Stopwatch stopwatch, Result<T, DomainError> result)
    {
        if (stopwatch.IsRunning)
            stopwatch.Stop();

        if (result.IsSuccess)
            TrackSuccess(action, stopwatch.Elapsed);
        else
            TrackFailure(action, stopwatch.Elapsed, result.Error);
    }


    private void TrackSuccess(string action, in TimeSpan elapsed) 
        => _entryInfoMetrics.TrackActionCompleted(action, EntryInfoMetricOutcomes.Success, elapsed, null);


    private void TrackFailure(string action, in TimeSpan elapsed, in DomainError error) 
        => _entryInfoMetrics.TrackActionCompleted(action, EntryInfoMetricOutcomes.Failure, elapsed, error.Code);


    private async Task<UnitResult<DomainError>> EnsureNotReported(Guid entryId, CancellationToken cancellationToken)
    {
        if (await _reportService.Contains(entryId, cancellationToken))
            return DomainErrors.EntryNotFound();

        return UnitResult.Success<DomainError>();
    }


    private async Task<Result<EntryInfo, DomainError>> GetEntryInfo(Guid entryId, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeyBuilder.BuildEntryInfoCacheKey(entryId);
        var result = await _dataStorage.TryGet<EntryInfo?>(cacheKey, cancellationToken);
        if (result is null)
            return DomainErrors.EntryNotFound();

        return result.Value;
    }


    private static bool IsBelongsToCreator(in EntryInfo entryInfo, in Creator creator)
        => entryInfo.CreatorId == creator.Id;


    private Task<UnitResult<DomainError>> RemoveAttachedImageInternally(Result<EntryInfo, DomainError> entryInfoResult, Creator creator, Guid entryId, Guid imageId, CancellationToken cancellationToken)
    {
        return entryInfoResult
            .Ensure(entryInfo => IsBelongsToCreator(entryInfo, creator), DomainErrors.NoPermissionError())
            .Bind(UpdateEntryInfo)
            .TapError(LogDomainError)
            .Bind(RemoveImage);


        Result<EntryInfo, DomainError> UpdateEntryInfo(EntryInfo entryInfo)
        { 
            entryInfo.ImageInfos.RemoveAll(imageInfo => imageInfo.Id == imageId);
            return entryInfo;
        }


        void LogDomainError(DomainError error)
            => _logger.LogImageRemovalDomainError(imageId, error.Detail!);


        async Task<UnitResult<DomainError>> RemoveImage(EntryInfo entry)
        {
            var removalResult = await _imageService.Remove(entryId, imageId, cancellationToken);
            if (removalResult.IsFailure)
                return removalResult;

            await PersistEntryInfo(entry);
            return removalResult;
        }


        async Task PersistEntryInfo(EntryInfo entryInfo)
        {
            var expiresAt = entryInfo.ExpiresAt - DateTimeOffset.UtcNow;
            var cacheKey = CacheKeyBuilder.BuildEntryInfoCacheKey(entryInfo.Id);
            await _dataStorage.Set(cacheKey, entryInfo, expiresAt, cancellationToken);

            var imageIds = entryInfo.ImageInfos.Select(info => info.Id);
            await _entryImageLifecycleService.Track(entryInfo.Id, entryInfo.ExpiresAt, imageIds, cancellationToken);
        }
    }


    private async Task<UnitResult<DomainError>> RemoveUnattachedImageInternally(Guid entryId, Guid imageId, CancellationToken cancellationToken)
    {
        var result = await _imageService.Remove(entryId, imageId, cancellationToken);
        if (result.IsSuccess)
            await _entryImageLifecycleService.RemoveImage(entryId, imageId, cancellationToken);

        return result;
    }


    private static string StripHtmlTags(string html)
    {
        var textWithoutTags = HtmlTagsExpression().Replace(html, " ");
        return WebUtility.HtmlDecode(textWithoutTags);
    }


    [GeneratedRegex(@"<(?:[^""'<>]|""[^""]*""|'[^']*')*?>", RegexOptions.Compiled)]
    private static partial Regex HtmlTagsExpression();


    private readonly ICreatorService _creatorService;
    private readonly IDataStorage _dataStorage;
    private readonly IEntryImageLifecycleService _entryImageLifecycleService;
    private readonly IEntryService _entryService;
    private readonly IImageService _imageService;
    private readonly IEntryInfoMetrics _entryInfoMetrics;
    private readonly ILogger<EntryInfoService> _logger;
    private readonly IOpenGraphService _openGraphService;
    private readonly IReportService _reportService;
    private readonly IViewCountService _viewCountService;
}
