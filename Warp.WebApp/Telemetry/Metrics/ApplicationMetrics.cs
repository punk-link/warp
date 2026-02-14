using System.Diagnostics.Metrics;

namespace Warp.WebApp.Telemetry.Metrics;

public static class ApplicationMetrics
{
    public static readonly Meter Meter = new("Warp.WebApp", "1.0");

    public static readonly Counter<int> RobotsMiddlewareCounter = Meter.CreateCounter<int>("robots_middleware_counter");

    public static readonly Counter<long> EntryInfoActionCounter = Meter.CreateCounter<long>("entry_info_action_total");

    public static readonly Histogram<double> EntryInfoActionDuration = Meter.CreateHistogram<double>("entry_info_action_duration_ms", "ms");

    public static readonly Histogram<int> EntryContentSizeChars = Meter.CreateHistogram<int>("entry_content_size_chars", "characters");
}
