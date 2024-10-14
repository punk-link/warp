using System.Diagnostics.Metrics;

namespace Warp.WebApp.Telemetry.Metrics;

public static class ApplicationMetrics
{
    public static readonly Meter Meter = new("Warp.WebApp", "1.0");

    public static readonly Counter<int> RobotsMiddlewareCounter = Meter.CreateCounter<int>("RobotsMiddlewareCounter");
}
