using System.ComponentModel.DataAnnotations;

namespace Warp.WebApp.Data.S3;

public class S3Options
{
    [Required]
    public required string AccessKey { get; set; }

    [Required]
    public required string SecretAccessKey { get; set; }

    [Required]
    public required string BucketName { get; set; }
}
