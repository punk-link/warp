using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Warp.WebApp.Data;
using Warp.WebApp.Data.S3;
using Warp.WebApp.Models.Entries;
using Warp.WebApp.Models.Options;
using Warp.WebApp.Telemetry.Logging;

namespace Warp.WebApp.Services.Images;

/// <summary>
/// Hosted service that periodically scans S3 for orphaned images and removes them.
/// </summary>
public sealed class OrphanImageCleanupService : BackgroundService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrphanImageCleanupService"/> class.
    /// </summary>
    /// <param name="scopeFactory">Factory used to create service scopes during cleanup runs.</param>
    /// <param name="amazonS3Factory">Factory that creates S3 clients.</param>
    /// <param name="options">Cleanup scheduling options.</param>
    /// <param name="logger">Logger instance for emitting telemetry.</param>
    public OrphanImageCleanupService(
        IServiceScopeFactory scopeFactory,
        IAmazonS3Factory amazonS3Factory,
        IOptions<OrphanImageCleanupOptions> options,
        ILogger<OrphanImageCleanupService> logger)
    {
        _amazonS3Factory = amazonS3Factory;
        _logger = logger;
        _options = options.Value;
        _scopeFactory = scopeFactory;
    }


    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var initialDelay = TimeSpan.FromMinutes(Math.Max(0, _options.InitialDelayMinutes));
        if (initialDelay > TimeSpan.Zero)
            await Task.Delay(initialDelay, stoppingToken);

        var interval = TimeSpan.FromHours(Math.Max(1, _options.RunIntervalHours));
        while (!stoppingToken.IsCancellationRequested)
        {
            await RunCleanup(stoppingToken);

            await Task.Delay(interval, stoppingToken);
        }
    }


    private async Task RunCleanup(CancellationToken cancellationToken)
    {
        _logger.LogOrphanImageCleanupStarted();

        var startedAt = DateTimeOffset.UtcNow;

        try
        {
            var (deletedCount, scannedCount) = await CleanupOrphans(cancellationToken);
            var elapsedSeconds = (DateTimeOffset.UtcNow - startedAt).TotalSeconds;

            _logger.LogOrphanImageCleanupCompleted(deletedCount, scannedCount, elapsedSeconds);
        }
        catch (OperationCanceledException)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogOrphanImageCleanupFailed(ex, ex.Message);
        }
    }


    private async Task<(int DeletedCount, int ScannedCount)> CleanupOrphans(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        using var s3Client = _amazonS3Factory.CreateClient();

        var dataStorage = scope.ServiceProvider.GetRequiredService<IDataStorage>();
        var lifecycleService = scope.ServiceProvider.GetRequiredService<IEntryImageLifecycleService>();
        var imageService = scope.ServiceProvider.GetRequiredService<IImageService>();

        var bucketName = _amazonS3Factory.GetBucketName();
        var pageSize = Math.Clamp(_options.PageSize, 1, 1000);
        var maxDeletes = Math.Max(0, _options.MaxDeletesPerRun);

        var deletedCount = 0;
        var scannedCount = 0;
        var continuationToken = default(string?);
        var entryReferenceCache = new Dictionary<Guid, bool>();

        do
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = new ListObjectsV2Request
            {
                BucketName = bucketName,
                ContinuationToken = continuationToken,
                MaxKeys = pageSize
            };

            var response = await s3Client.ListObjectsV2Async(request, cancellationToken);
            var objects = response.S3Objects;
            if (objects is null || objects.Count == 0)
                break;

            foreach (var s3Object in objects)
            {
                cancellationToken.ThrowIfCancellationRequested();

                scannedCount++;
                if (!TryParseObjectKey(s3Object.Key, out var entryId, out var imageId))
                    continue;

                if (!entryReferenceCache.TryGetValue(entryId, out var hasReferences))
                {
                    hasReferences = await HasActiveReferences(entryId, dataStorage, lifecycleService, cancellationToken);
                    entryReferenceCache[entryId] = hasReferences;
                }

                if (hasReferences)
                    continue;

                var removeResult = await imageService.Remove(entryId, imageId, cancellationToken);
                if (removeResult.IsFailure)
                {
                    _logger.LogEntryImageCleanupFailure(imageId, entryId, removeResult.Error.Detail);
                    continue;
                }

                deletedCount++;
                if (maxDeletes > 0 && deletedCount >= maxDeletes)
                    return (deletedCount, scannedCount);
            }

            continuationToken = response.IsTruncated.GetValueOrDefault()
                ? response.NextContinuationToken
                : null;
        }
        while (!string.IsNullOrEmpty(continuationToken));

        return (deletedCount, scannedCount);
    }


    private static async Task<bool> HasActiveReferences(Guid entryId, IDataStorage dataStorage, IEntryImageLifecycleService lifecycleService, CancellationToken cancellationToken)
    {
        var lifecycle = await lifecycleService.Get(entryId, cancellationToken);
        if (lifecycle is not null)
            return true;

        var cacheKey = CacheKeyBuilder.BuildEntryInfoCacheKey(entryId);
        return await dataStorage.Contains<EntryInfo>(cacheKey, cancellationToken);
    }


    private static bool TryParseObjectKey(string? key, out Guid entryId, out Guid imageId)
    {
        entryId = Guid.Empty;
        imageId = Guid.Empty;

        if (string.IsNullOrWhiteSpace(key))
            return false;

        var segments = key.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2)
            return false;

        if (!Guid.TryParse(segments[0], out entryId))
            return false;

        return Guid.TryParse(segments[^1], out imageId);
    }


    private readonly IAmazonS3Factory _amazonS3Factory;
    private readonly ILogger<OrphanImageCleanupService> _logger;
    private readonly OrphanImageCleanupOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;
}
