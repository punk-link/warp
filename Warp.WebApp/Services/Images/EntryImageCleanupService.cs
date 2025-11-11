using Microsoft.Extensions.Options;
using Warp.WebApp.Models.Images;
using Warp.WebApp.Models.Options;
using Warp.WebApp.Telemetry.Logging;

namespace Warp.WebApp.Services.Images;

/// <summary>
/// Hosted service responsible for removing expired entry images on a scheduled basis.
/// </summary>
public sealed class EntryImageCleanupService : BackgroundService
{
    public EntryImageCleanupService(IServiceScopeFactory scopeFactory, IOptions<EntryCleanupOptions> options, ILogger<EntryImageCleanupService> logger)
    {
        _logger = logger;
        _options = options.Value;
        _scopeFactory = scopeFactory;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var delay = TimeSpan.FromSeconds(Math.Max(1, _options.DelayBetweenBatchesSeconds));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
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
                _logger.LogEntryImageCleanupBatchError(ex, ex.Message);
                await Task.Delay(delay, stoppingToken);
            }
        }
    }


    private async Task<bool> ProcessBatch(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var lifecycleService = scope.ServiceProvider.GetRequiredService<IEntryImageLifecycleService>();
        var imageService = scope.ServiceProvider.GetRequiredService<IImageService>();

        var expired = await lifecycleService.TakeExpired(DateTimeOffset.UtcNow, _options.BatchSize, cancellationToken);
        if (expired.Count == 0)
            return false;

        foreach (var lifecycle in expired)
            await ProcessLifecycle(lifecycleService, imageService, lifecycle, cancellationToken);

        return true;
    }


    private async Task ProcessLifecycle(IEntryImageLifecycleService lifecycleService, IImageService imageService, EntryImageLifecycle lifecycle, CancellationToken cancellationToken)
    {
        if (!await lifecycleService.TryAcquireProcessingLock(lifecycle.EntryId, cancellationToken))
            return;

        try
        {
            if (lifecycle.ImageIds.Count == 0)
            {
                await lifecycleService.Remove(lifecycle.EntryId, cancellationToken);
                return;
            }

            var succeeded = true;
            foreach (var imageId in lifecycle.ImageIds)
            {
                var result = await imageService.Remove(lifecycle.EntryId, imageId, cancellationToken);
                if (result.IsFailure)
                {
                    succeeded = false;
                    _logger.LogEntryImageCleanupFailure(imageId, lifecycle.EntryId, result.Error.Detail);
                }
            }

            if (succeeded)
            {
                await lifecycleService.Remove(lifecycle.EntryId, cancellationToken);
                return;
            }

            if (lifecycle.FailureCount + 1 >= _options.FailureThreshold)
            {
                _logger.LogEntryImageCleanupAbandoned(lifecycle.EntryId, lifecycle.FailureCount + 1);
                await lifecycleService.Remove(lifecycle.EntryId, cancellationToken);
                return;
            }

            var retryAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(1, _options.FailureBackoffSeconds));
            await lifecycleService.Reschedule(lifecycle, retryAt, cancellationToken);
        }
        finally
        {
            await lifecycleService.ReleaseProcessingLock(lifecycle.EntryId, cancellationToken);
        }
    }


    private readonly ILogger<EntryImageCleanupService> _logger;
    private readonly EntryCleanupOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;
}
