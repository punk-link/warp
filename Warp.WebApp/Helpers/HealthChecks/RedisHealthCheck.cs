using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Globalization;
using Warp.WebApp.Helpers.Configuration;

namespace Warp.WebApp.Helpers.HealthChecks;

public class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _multiplexer;

    public RedisHealthCheck(IConnectionMultiplexer connectionMultiplexer)
    {
        _multiplexer = connectionMultiplexer;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_multiplexer.IsConnected
            ? new HealthCheckResult(HealthStatus.Healthy, "Redis state - OK")
            : new HealthCheckResult(HealthStatus.Unhealthy, "Redis connection is not working")
        );
    }
}
