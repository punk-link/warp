using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Warp.WebApp.Helpers.HealthChecks;

public class ControllerResolveHealthCheck : IHealthCheck
{
    public ControllerResolveHealthCheck(IServiceProvider provider)
    {
        _provider = provider;
    }


    // TODO: Remove if your issue number is bigger than 175
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (_areControllersResolved)
            return Task.FromResult(new HealthCheckResult(HealthStatus.Healthy));

        try
        {
            foreach (var controllerType in _controllerTypes)
                _provider.GetRequiredService(controllerType);

            _areControllersResolved = true;

            return Task.FromResult(new HealthCheckResult(HealthStatus.Healthy));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new HealthCheckResult(HealthStatus.Unhealthy, exception: ex));
        }
    }


    private static readonly Type[] _controllerTypes = typeof(ControllerResolveHealthCheck).Assembly
        .GetTypes()
        .Where(t => Attribute.GetCustomAttribute(t, typeof(ApiControllerAttribute)) is not null)
        .Where(t => t is { IsAbstract: false, IsPublic: true })
        .ToArray();


    private readonly IServiceProvider _provider;
    private static bool _areControllersResolved;
}