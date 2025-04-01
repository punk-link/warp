using System.Diagnostics;
using Warp.WebApp.Attributes;

namespace Warp.WebApp.Helpers.Warmups;

public class WarmupService : IHostedService
{
    public WarmupService(
        IServiceCollection serviceCollection, 
        IServiceProvider serviceProvider, 
        IRouteWarmer routeWarmer,
        ILogger<WarmupService> logger)
    {
        _logger = logger;
        _serviceCollection = serviceCollection;
        _serviceProvider = serviceProvider;
        _routeWarmer = routeWarmer;
    }


    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting application warmup process...");
        var stopwatch = Stopwatch.StartNew();
        
        // Warm up services
        await Task.Run(() => WarmUp(_serviceCollection, _serviceProvider, _logger, cancellationToken), cancellationToken);
        
        await _routeWarmer.WarmUpRoutes(cancellationToken);
        
        stopwatch.Stop();
        _logger.LogInformation("Application warmup completed in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
    }


    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Warmup service stopping");
        return Task.CompletedTask;
    }


    [TraceMethod]
    private static void WarmUp(IServiceCollection services, IServiceProvider serviceProvider, ILogger logger, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var serviceTypes = GetServiceTypes(services).ToList();
            
            logger.LogInformation("Warming up {Count} services", serviceTypes.Count);
            
            int warmedUpCount = 0;
            foreach (var serviceType in serviceTypes)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    logger.LogWarning("Warmup process was canceled before completion");
                    break;
                }
                
                try
                {
                    scope.ServiceProvider.GetService(serviceType);
                    warmedUpCount++;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to warm up service {ServiceType}", serviceType.FullName);
                }
            }
            
            logger.LogInformation("Successfully warmed up {WarmedUpCount} of {TotalCount} services",  warmedUpCount, serviceTypes.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during application warmup");
            throw;
        }
    }


    private static IEnumerable<Type> GetServiceTypes(IServiceCollection services)
        => services
            .Where(descriptor => descriptor.ImplementationType != typeof(WarmupService))
            .Where(descriptor => descriptor.ServiceType.ContainsGenericParameters is false)
            .Where(descriptor => descriptor.ServiceType.IsInterface)
            .Select(descriptor => descriptor.ServiceType)
            .Distinct();


    private readonly ILogger<WarmupService> _logger;
    private readonly IServiceCollection _serviceCollection;
    private readonly IServiceProvider _serviceProvider;
    private readonly IRouteWarmer _routeWarmer;
}