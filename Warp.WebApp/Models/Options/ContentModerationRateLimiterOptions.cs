using System.ComponentModel.DataAnnotations;

namespace Warp.WebApp.Models.Options;

/// <summary>
/// Configuration options for the adaptive rate limiter used when calling the content moderation API.
/// </summary>
public sealed class ContentModerationRateLimiterOptions
{
    /// <summary>Number of consecutive failures required to trigger circuit-breaker behaviour (immediate concurrency decrease).</summary>
    [Range(1, 50)]
    public int CircuitBreakerFailureThreshold { get; set; } = 5;

    /// <summary>Fraction (0.0–1.0) of current concurrency removed on each decrease step.</summary>
    [Range(0.01, 1.0)]
    public double ConcurrencyDecreaseRatio { get; set; } = 0.3;

    /// <summary>Step size by which concurrency is increased when success rate is healthy.</summary>
    [Range(1, 10)]
    public int ConcurrencyIncrement { get; set; } = 1;

    /// <summary>Fraction (0.0–1.0) of current concurrency removed on each decrease step.</summary>
    [Range(0.01, 1.0)]
    public double ConcurrencyDecreaseRatio { get; set; } = 0.3;

    /// <summary>
    /// Multiplier applied to <see cref="ContentModerationOptions.SuccessThreshold"/> to determine the lower bound
    /// at which concurrency should be decreased.
    /// </summary>
    [Range(0.01, 1.0)]
    public double LowSuccessRateMultiplier { get; set; } = 0.9;

    /// <summary>Floor value for concurrency — it will never drop below this.</summary>
    [Range(1, 10)]
    public int MinimumConcurrency { get; set; } = 1;

    /// <summary>Minimum amount by which concurrency is decreased in a single step.</summary>
    [Range(1, 10)]
    public int MinimumConcurrencyDecrement { get; set; } = 1;

    /// <summary>Minimum number of samples required in the sliding window before concurrency is adjusted.</summary>
    [Range(1, 100)]
    public int MinimumSampleSize { get; set; } = 10;
}
