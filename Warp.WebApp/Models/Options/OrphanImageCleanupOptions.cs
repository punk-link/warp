using System.ComponentModel.DataAnnotations;

namespace Warp.WebApp.Models.Options;

/// <summary>
/// Configures the execution cadence and limits for orphan image cleanup.
/// </summary>
public sealed class OrphanImageCleanupOptions
{
    /// <summary>
    /// Interval, in hours, between orphan image cleanup runs.
    /// </summary>
    [Range(1, 168)]
    public int RunIntervalHours { get; set; } = 24;


    /// <summary>
    /// Initial delay, in minutes, applied once before the first cleanup run.
    /// </summary>
    [Range(0, 1440)]
    public int InitialDelayMinutes { get; set; } = 30;


    /// <summary>
    /// Maximum number of objects requested from S3 per listing operation.
    /// </summary>
    [Range(1, 1000)]
    public int PageSize { get; set; } = 500;


    /// <summary>
    /// Maximum number of orphaned images removed during a single cleanup execution. Set to 0 for no limit.
    /// </summary>
    [Range(0, 10000)]
    public int MaxDeletesPerRun { get; set; } = 500;
}
