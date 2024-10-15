using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Warp.WebApp.Telemetry.Tracing;

public static class TracingExtensions
{
    public static OpenTelemetryBuilder AddTracing(this OpenTelemetryBuilder builder, string serviceName)
    {
        builder.WithTracing(tracing =>
        {
            tracing.ConfigureResource(configure => configure.AddService(serviceName))
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.Filter = (httpContext) => _excludedEndpoints.Any(endpoint => { 
                        return !httpContext.Request.Path.ToString().AsSpan().StartsWith(endpoint);
                    });
                })
                .AddRedisInstrumentation()
                .SetSampler(new AlwaysOnSampler());
        });

        return builder;
    }

    private static readonly HashSet<string> _excludedEndpoints =
    [
        "/metrics",
        "/health"
    ];
}
