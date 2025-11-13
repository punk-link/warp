using System.Diagnostics;
using Warp.WebApp.Constants.Logging;

namespace Warp.WebApp.Telemetry.Metrics;

/// <summary>
/// Tracks metrics for entry info related actions.
/// </summary>
public sealed class EntryInfoMetrics : IEntryInfoMetrics
{
    /// <inheritdoc/>
    public void TrackActionCompleted(string action, string outcome, TimeSpan duration, LogEvents? failureReason = null)
    {
        var tags = new TagList
        {
            { EntryInfoMetricTagNames.Action, action },
            { EntryInfoMetricTagNames.Outcome, outcome }
        };

        if (failureReason.HasValue)
            tags.Add(EntryInfoMetricTagNames.FailureReason, failureReason.Value.ToString());

        ApplicationMetrics.EntryInfoActionCounter.Add(1, tags);
        ApplicationMetrics.EntryInfoActionDuration.Record(duration.TotalMilliseconds, tags);
    }
}
