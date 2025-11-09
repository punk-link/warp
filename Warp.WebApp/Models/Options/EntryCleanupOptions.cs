using System.ComponentModel.DataAnnotations;

namespace Warp.WebApp.Models.Options;

/// <summary>
/// Configuration options for background cleanup of expired entry images.
/// </summary>
public sealed class EntryCleanupOptions
{
    [Range(1, 500)]
    public int BatchSize { get; set; } = 50;


    [Range(1, 3600)]
    public int DelayBetweenBatchesSeconds { get; set; } = 60;


    [Range(1, 3600)]
    public int FailureBackoffSeconds { get; set; } = 300;


    [Range(1, 20)]
    public int FailureThreshold { get; set; } = 5;


    [Range(1, 1440)]
    public int MetadataRetentionBufferMinutes { get; set; } = 60;


    [Range(10, 600)]
    public int ProcessingLockSeconds
    {
        get => _processingLockSeconds;
        set => _processingLockSeconds = Math.Max(10, value);
    }


    private int _processingLockSeconds = 60;
}
