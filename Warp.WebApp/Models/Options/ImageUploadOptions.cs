namespace Warp.WebApp.Models.Options;

public class ImageUploadOptions
{
    public long MaxFileSize { get; set; } = 50 * 1024 * 1024;
    public int MaxFileCount { get; set; } = 10;
    public string[] AllowedExtensions { get; set; } = [".bmp", ".gif", ".ico", ".jpeg", ".jpg", ".png", ".svg", ".tiff", ".webp"];
    // The spec at https://tools.ietf.org/html/rfc2046#section-5.1 states that 70 characters is a reasonable limit.
    public int RequestBoundaryLengthLimit { get; set; } = 70;
}
