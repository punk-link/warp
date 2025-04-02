using System.Diagnostics;

namespace Warp.WebApp.Helpers.Warmups;

/// <summary>
/// Service that handles the warmup process for the application.
/// Waits for the application to fully start before initiating warmup.
/// </summary>
public class WarmupService : IHostedService
{
    public WarmupService(
        ILogger<WarmupService> logger,
        IHostApplicationLifetime applicationLifetime,
        IServiceWarmer serviceWarmer,
        IRouteWarmer routeWarmer)
    {
        _applicationLifetime = applicationLifetime;
        _logger = logger;
        _serviceWarmer = serviceWarmer;
        _routeWarmer = routeWarmer;
    }


    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                // Wait for application startup
                var startedTcs = new TaskCompletionSource<object>();
                using var registration = _applicationLifetime.ApplicationStarted.Register(() => startedTcs.TrySetResult(null));
                await startedTcs.Task;
            
                // Delay briefly to allow the application to stabilize after startup
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            
                _logger.LogInformation("Starting application warmup process...");
                var stopwatch = Stopwatch.StartNew();
            
                using var warmupCts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, warmupCts.Token);

                await _serviceWarmer.WarmUpServices(linkedCts.Token);
                await _routeWarmer.WarmUpRoutes(linkedCts.Token);
            
                stopwatch.Stop();
                _logger.LogInformation($"Application warmup completed in {stopwatch.ElapsedMilliseconds}ms");
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Application warmup was canceled or timed out");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during application warmup");
            }
        });

        return Task.CompletedTask;
    }


    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Warmup service stopping");
        return Task.CompletedTask;
    }


    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly ILogger<WarmupService> _logger;
    private readonly IServiceWarmer _serviceWarmer;
    private readonly IRouteWarmer _routeWarmer;
}