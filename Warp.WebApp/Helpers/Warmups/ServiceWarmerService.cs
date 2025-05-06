using Warp.WebApp.Attributes;

namespace Warp.WebApp.Helpers.Warmups;

/// <summary>
/// Implementation of <see cref="IServiceWarmer"/> that instantiates services from the DI container
/// </summary>
public class ServiceWarmerService : IServiceWarmer
{
    public ServiceWarmerService(
        IServiceCollection serviceCollection,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory)
    {
        _serviceCollection = serviceCollection;
        _serviceProvider = serviceProvider;
        _logger = loggerFactory.CreateLogger<ServiceWarmerService>();
    }


    /// <inheritdoc />
    [TraceMethod]
    public async Task WarmUpServices(CancellationToken cancellationToken)
    {
        try
        {
            var serviceTypes = GetServiceTypes(_serviceCollection);
            _logger.LogInformation("Warming up {Count} services in parallel", serviceTypes.Count);
            
            var successCount = 0;
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount * 2,
                CancellationToken = cancellationToken
            };

            await Parallel.ForEachAsync(serviceTypes, parallelOptions, async (serviceType, ct) =>
            {
                try
                {
                    using var serviceScope = _serviceProvider.CreateScope();
                    await Task.Run(() => serviceScope.ServiceProvider.GetService(serviceType), ct);
                    Interlocked.Increment(ref successCount);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to warm up service {FullName}", serviceType.FullName);
                }
            });
            
            _logger.LogInformation("Successfully warmed up {SuccessCount} of {Count} services", successCount, serviceTypes.Count);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Service warmup was canceled before completion");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during service warmup");
            throw;
        }
    }


    private static List<Type> GetServiceTypes(IServiceCollection services)
        => services
            .Where(descriptor => 
                descriptor.ServiceType != typeof(IHostedService) && 
                !descriptor.ServiceType.IsAssignableTo(typeof(IRouteWarmer)) && 
                !descriptor.ServiceType.IsAssignableTo(typeof(IServiceWarmer)))
            .Where(descriptor => descriptor.ServiceType.ContainsGenericParameters is false)
            .Where(descriptor => descriptor.ServiceType.IsInterface)
            .Select(descriptor => descriptor.ServiceType)
            .Distinct()
            .ToList();


    private readonly IServiceCollection _serviceCollection;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ServiceWarmerService> _logger;
}
