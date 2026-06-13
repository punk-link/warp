using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace Warp.WebApp.Models.Options;

/// <summary>
/// Configuration options for the asynchronous content moderation pipeline.
/// </summary>
public sealed class ContentModerationOptions
{
    /// <summary>
    /// API key used to authorize moderation requests.
    /// Prefer providing this via secrets or environment configuration rather than checked-in settings files.
    /// </summary>
    [Required]
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Maximum number of jobs processed in a single worker cycle.</summary>
    [Range(1, 100)]
    public int BatchSize { get; set; } = 10;

    /// <summary>Seconds the worker sleeps between batches when the queue is empty.</summary>
    [Range(1, 3600)]
    public int DelayBetweenBatchesSeconds { get; set; } = 5;

    /// <summary>Base URL of the OpenAI-compatible moderation API endpoint.</summary>
    [Required]
    public string Endpoint { get; set; } = "https://api.openai.com/v1";

    /// <summary>Seconds to wait before retrying a failed job.</summary>
    [Range(1, 3600)]
    public int FailureBackoffSeconds { get; set; } = 300;

    /// <summary>Number of consecutive failures after which a job is abandoned.</summary>
    [Range(1, 20)]
    public int FailureThreshold { get; set; } = 5;

    /// <summary>Initial number of concurrent moderation API calls allowed.</summary>
    [Range(1, 20)]
    public int InitialConcurrency { get; set; } = 2;

    /// <summary>Maximum number of concurrent moderation API calls allowed.</summary>
    [Range(1, 20)]
    public int MaxConcurrency { get; set; } = 5;

    /// <summary>
    /// Extra time added to the entry TTL when calculating how long to keep job metadata.
    /// Prevents premature eviction of jobs for entries that are close to expiry.
    /// </summary>
    [Range(1, 1440)]
    public int MetadataRetentionBufferMinutes { get; set; } = 60;

    /// <summary>
    /// Identifier of the moderation model to use.
    /// Defaults to the latest OpenAI omni-moderation model.
    /// </summary>
    [Required]
    public string Model { get; set; } = "omni-moderation-latest";

    /// <summary>
    /// Duration in seconds for the per-job processing lock.
    /// Should be long enough to cover the worst-case moderation time for a full batch of images.
    /// </summary>
    [Range(10, 600)]
    public int ProcessingLockSeconds { get; set; } = 120;

    /// <summary>Fine-grained options for the adaptive rate limiter algorithm.</summary>
    [ValidateObjectMembers]
    public ContentModerationRateLimiterOptions RateLimiter { get; set; } = new();

    /// <summary>Sliding window duration used to calculate the success rate.</summary>
    public TimeSpan SuccessRateWindow { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Minimum success rate (0.01–1.0) in the sliding window before concurrency is increased.
    /// </summary>
    [Range(0.01, 1.0)]
    public double SuccessThreshold { get; set; } = 0.95;
}
