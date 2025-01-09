using System.ComponentModel.DataAnnotations;

namespace Warp.WebApp.Models.Options;

public class ImageUploadOptions
{
    [Required]
    [Range(1, long.MaxValue)]
    public long MaxFileSize { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int MaxFileCount { get; set; }

    public string[] AllowedExtensions { get; set; }

    // The spec at https://tools.ietf.org/html/rfc2046#section-5.1 states that 70 characters is a reasonable limit.
    [Required]
    [Range(1, int.MaxValue)]
    public int RequestBoundaryLengthLimit { get; set; }
}
