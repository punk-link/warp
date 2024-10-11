using Warp.WebApp.Attributes;

namespace Warp.WebApp.Helpers.Warmups;

public class WarmupService : IHostedService
{
    public WarmupService(IServiceCollection serviceCollection, IServiceProvider serviceProvider)
    {
        _serviceCollection = serviceCollection;
        _serviceProvider = serviceProvider;
    }


    public Task StartAsync(CancellationToken cancellationToken)
    {
        WarmUp(_serviceCollection, _serviceProvider);

        return Task.CompletedTask;
    }


    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;


    [TraceMethod]
    private static void WarmUp(IServiceCollection services, IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        foreach (var serviceType in GetServiceTypes(services))
            scope.ServiceProvider.GetService(serviceType);
    }


    private static IEnumerable<Type> GetServiceTypes(IServiceCollection services)
        => services
            .Where(descriptor => descriptor.ImplementationType != typeof(WarmupService))
            .Where(descriptor => descriptor.ServiceType.ContainsGenericParameters is false)
            .Select(descriptor => descriptor.ServiceType)
            .Distinct();


    private readonly IServiceCollection _serviceCollection;
    private readonly IServiceProvider _serviceProvider;
}