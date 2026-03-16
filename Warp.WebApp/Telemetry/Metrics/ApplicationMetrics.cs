using System.Diagnostics.Metrics;

namespace Warp.WebApp.Telemetry.Metrics;

public static class ApplicationMetrics
{
    public static readonly Meter Meter = new("Warp.WebApp", "1.0");

    public static readonly Counter<int> RobotsMiddlewareCounter = Meter.CreateCounter<int>("robots_middleware_counter");

    public static readonly Counter<long> EntryInfoActionCounter = Meter.CreateCounter<long>("entry_info_action_total");

    public static readonly Histogram<double> EntryInfoActionDuration = Meter.CreateHistogram<double>("entry_info_action_duration_ms", "ms");

    public static readonly Histogram<int> EntryContentSizeChars = Meter.CreateHistogram<int>("entry_content_size_chars", "characters");

    public static readonly Counter<long> ImagesScannedTotal = Meter.CreateCounter<long>("images_scanned_total");

    public static readonly Counter<long> MaliciousImagesDetectedTotal = Meter.CreateCounter<long>("malicious_images_detected_total");

    public static readonly Counter<long> EntriesBlockedByMalwareTotal = Meter.CreateCounter<long>("entries_blocked_by_malware_total");

    public static readonly Counter<long> EntriesWithMaliciousImagesTotal = Meter.CreateCounter<long>("entries_with_malicious_images_total");
}
