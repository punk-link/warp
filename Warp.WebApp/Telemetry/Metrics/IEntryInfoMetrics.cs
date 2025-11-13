using Warp.WebApp.Constants.Logging;

namespace Warp.WebApp.Telemetry.Metrics;

/// <summary>
/// Describes the contract for recording metrics associated with entry info actions.
/// </summary>
public interface IEntryInfoMetrics
{
    /// <summary>
    /// Tracks the completion of an entry info action.
    /// </summary>
    /// <param name="action">The action identifier.</param>
    /// <param name="outcome">The outcome identifier.</param>
    /// <param name="duration">The action duration.</param>
    /// <param name="failureReason">Optional failure reason.</param>
    void TrackActionCompleted(string action, string outcome, TimeSpan duration, LogEvents? failureReason = null);
}
