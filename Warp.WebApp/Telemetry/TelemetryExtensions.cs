using OpenTelemetry;
using OpenTelemetry.Exporter;
using Warp.WebApp.Telemetry.Metrics;
using Warp.WebApp.Telemetry.Tracing;

namespace Warp.WebApp.Telemetry;

public static class TelemetryExtensions
{
    public static WebApplicationBuilder AddTelemetry(this WebApplicationBuilder builder)
    {
        var openTelemetryEndpoint = builder.Configuration["OpenTelemetry:Endpoint"]!;
    if (string.IsNullOrWhiteSpace(openTelemetryEndpoint))
            return builder;

    var serviceName = builder.Configuration["ServiceName"]!;
    _ = builder.Services
        .AddOpenTelemetry()
        .AddMetrics(serviceName)
        .AddTracing(serviceName)
        .UseOtlpExporter(OtlpExportProtocol.Grpc, new Uri(openTelemetryEndpoint));

    return builder;
    }
}
