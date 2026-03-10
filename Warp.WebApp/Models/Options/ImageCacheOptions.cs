using System.ComponentModel.DataAnnotations;

namespace Warp.WebApp.Models.Options;

/// <summary>
/// Configuration options for image binary caching in KeyDB.
/// </summary>
public class ImageCacheOptions
{
    /// <summary>
    /// The maximum file size in bytes eligible for KeyDB caching.
    /// Images exceeding this threshold are served directly from S3.
    /// </summary>
    [Required]
    [Range(1, long.MaxValue)]
    public long MaxCachableFileSize { get; set; }
}
