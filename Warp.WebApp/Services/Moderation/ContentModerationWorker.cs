using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Warp.WebApp.Data;
using Warp.WebApp.Models.Entries;
using Warp.WebApp.Models.Files;
using Warp.WebApp.Models.Images;
using Warp.WebApp.Models.Moderation;
using Warp.WebApp.Models.Options;
using Warp.WebApp.Telemetry.Logging;

namespace Warp.WebApp.Services.Moderation;

/// <summary>
/// Hosted service that dequeues pending content moderation jobs, calls the moderation provider
/// for text and images, and persists the results back into the entry cache.
/// </summary>
public sealed class ContentModerationWorker : BackgroundService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentModerationWorker"/> class.
    /// </summary>
    /// <param name="scopeFactory">Factory used to create service scopes during processing.</param>
    /// <param name="featureManager">Feature manager used to check whether content moderation is enabled.</param>
    /// <param name="options">Moderation configuration options.</param>
    /// <param name="logger">Logger for emitting telemetry.</param>
    public ContentModerationWorker(
        IServiceScopeFactory scopeFactory,
        IFeatureManager featureManager,
        IOptions<ContentModerationOptions> options,
        ILogger<ContentModerationWorker> logger)
    {
        _featureManager = featureManager;
        _logger = logger;
        _options = options.Value;
        _scopeFactory = scopeFactory;
    }


    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var delay = TimeSpan.FromSeconds(Math.Max(1, _options.DelayBetweenBatchesSeconds));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!await _featureManager.IsEnabledAsync(FeatureName))
                {
                    await Task.Delay(delay, stoppingToken);
                    continue;
                }

                var processed = await ProcessBatch(stoppingToken);
                if (!processed)
                    await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;
            }
            catch (Exception ex)
            {
                _logger.LogContentModerationWorkerBatchError(ex, ex.Message);
                await Task.Delay(delay, stoppingToken);
            }
        }
    }


    private async Task<bool> ProcessBatch(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var jobService = scope.ServiceProvider.GetRequiredService<IModerationJobService>();
        var moderationService = scope.ServiceProvider.GetRequiredService<IContentModerationService>();
        var dataStorage = scope.ServiceProvider.GetRequiredService<IDataStorage>();

        var members = await jobService.TakeDue(DateTimeOffset.UtcNow, _options.BatchSize, cancellationToken);
        if (members.Count == 0)
            return false;

        foreach (var member in members)
            await ProcessMember(jobService, moderationService, dataStorage, member, cancellationToken);

        return true;
    }


    private async Task ProcessMember(
        IModerationJobService jobService,
        IContentModerationService moderationService,
        IDataStorage dataStorage,
        string member,
        CancellationToken cancellationToken)
    {
        var job = await jobService.GetByMember(member, cancellationToken);
        if (job is null)
        {
            await jobService.RemoveMember(member, cancellationToken);
            return;
        }

        if (!await jobService.TryAcquireProcessingLock(job.EntryId, cancellationToken))
            return;

        try
        {
            await ProcessJob(jobService, moderationService, dataStorage, job, cancellationToken);
        }
        finally
        {
            await jobService.ReleaseProcessingLock(job.EntryId, cancellationToken);
        }
    }


    private async Task ProcessJob(
        IModerationJobService jobService,
        IContentModerationService moderationService,
        IDataStorage dataStorage,
        EntryModerationJob job, 
        CancellationToken cancellationToken)
    {
        var entryInfoKey = CacheKeyBuilder.BuildEntryInfoCacheKey(job.EntryId);
        var entryInfo = await dataStorage.TryGet<EntryInfo?>(entryInfoKey, cancellationToken);
        if (entryInfo is null)
        {
            await jobService.Remove(job, cancellationToken);
            return;
        }

        var currentEntryInfo = entryInfo.Value;
        var (textModerationResult, textFailed) = await ModerateText(currentEntryInfo);
        var (updatedImages, failedImageIds) = await ModerateImages(currentEntryInfo.ImageInfos);

        await PersistUpdatedEntry(currentEntryInfo, entryInfoKey, textModerationResult, updatedImages);
        await HandleOutcome(textFailed, failedImageIds);

        return;


        async Task<(ModerationResult? result, bool failed)> ModerateText(EntryInfo info)
        {
            if (!job.NeedsTextModeration)
                return (info.TextModerationResult, false);

            var result = await moderationService.ModerateText(ExtractPlainText(info), cancellationToken);
            var failed = result.Status == ModerationStatus.Failed;

            if (!failed && result.IsFlagged)
                _logger.LogTextContentFlagged(job.EntryId);

            return (result, failed);
        }


        async Task<(List<ImageInfo> updatedImages, List<Guid> failedIds)> ModerateImages(IEnumerable<ImageInfo> imageInfos)
        {
            var updated = new List<ImageInfo>(imageInfos);
            var failedIds = new List<Guid>();

            foreach (var imageId in job.PendingImageIds)
            {
                var imageResult = await ModerateImage(moderationService, dataStorage, job.EntryId, imageId, cancellationToken);
                if (imageResult.Status == ModerationStatus.Failed)
                {
                    failedIds.Add(imageId);
                    continue;
                }

                if (imageResult.IsFlagged)
                    _logger.LogImageContentFlagged(imageId, job.EntryId);

                var index = updated.FindIndex(img => img.Id == imageId);
                if (index >= 0)
                    updated[index] = updated[index] with { ModerationResult = imageResult };
            }

            return (updated, failedIds);
        }


        async Task PersistUpdatedEntry(EntryInfo info, string cacheKey, ModerationResult? textResult, List<ImageInfo> images)
        {
            var remainingTtl = info.ExpiresAt - DateTimeOffset.UtcNow;
            if (remainingTtl <= TimeSpan.Zero)
                return;

            var updated = info with { ImageInfos = images, TextModerationResult = textResult };
            await dataStorage.Set(cacheKey, updated, remainingTtl, cancellationToken);
        }


        async Task HandleOutcome(bool textFailed, List<Guid> failedIds)
        {
            if (!textFailed && failedIds.Count == 0)
            {
                await jobService.Remove(job, cancellationToken);
                return;
            }

            if (job.FailureCount + 1 >= _options.FailureThreshold)
            {
                _logger.LogContentModerationJobAbandoned(job.EntryId, job.FailureCount + 1);
                await jobService.Remove(job, cancellationToken);
                return;
            }

            var retryAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(1, _options.FailureBackoffSeconds));
            var remainingJob = job.WithRemainingWork(textFailed, failedIds);
            await jobService.Reschedule(remainingJob, retryAt, cancellationToken);
        }
    }


    private async Task<ModerationResult> ModerateImage(
        IContentModerationService moderationService,
        IDataStorage dataStorage,
        Guid entryId,
        Guid imageId,
        CancellationToken cancellationToken)
    {
        try
        {
            var imageCacheKey = CacheKeyBuilder.BuildImageContentCacheKey(entryId, imageId);
            var cachedImageNullable = await dataStorage.TryGet<CachedImage>(imageCacheKey, cancellationToken);
            if (cachedImageNullable is not { Content.Length: > 0 } cachedImage)
                return ModerationResult.CreateFailed();

            return await moderationService.ModerateImage(cachedImage.Content, cachedImage.ContentType, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogContentModerationProviderError(entryId, ex.Message);
            return ModerationResult.CreateFailed();
        }
    }


    private static string ExtractPlainText(EntryInfo entryInfo)
    {
        var content = entryInfo.Entry.Content;
        if (string.IsNullOrWhiteSpace(content))
            return string.Empty;

        return HtmlSanitizer.GetPlainText(content);
    }


    /// <summary>The feature flag name that gates content moderation processing.</summary>
    public const string FeatureName = "ContentModeration";


    private readonly IFeatureManager _featureManager;
    private readonly ILogger<ContentModerationWorker> _logger;
    private readonly ContentModerationOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;
}
