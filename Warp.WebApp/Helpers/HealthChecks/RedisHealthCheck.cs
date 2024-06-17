using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace Warp.WebApp.Helpers.HealthChecks;

public class RedisHealthCheck : IHealthCheck
{
    public RedisHealthCheck(IConnectionMultiplexer connectionMultiplexer)
    {
        _multiplexer = connectionMultiplexer;
    }


    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
        => Task.FromResult(_multiplexer.IsConnected
            ? new HealthCheckResult(HealthStatus.Healthy, "Redis state - OK")
            : new HealthCheckResult(HealthStatus.Unhealthy, "Redis connection is not working")
        );


    private readonly IConnectionMultiplexer _multiplexer;
}
