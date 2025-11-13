using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace Warp.WebApp.Telemetry.Metrics;

public static class MetricsExtensions
{
    public static OpenTelemetryBuilder AddMetrics(this OpenTelemetryBuilder builder, string serviceName)
    {
        builder.WithMetrics(metrics =>
        {
            metrics.ConfigureResource(configure => configure.AddService(serviceName))
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddMeter("Microsoft.AspNetCore.Hosting")
                .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                .AddMeter(ApplicationMetrics.Meter.Name);
        });

        return builder;
    }
}
