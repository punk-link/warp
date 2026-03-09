namespace Warp.WebApp.Models.Files;

/// <summary>
/// Represents a cached image binary stored in KeyDB.
/// </summary>
public readonly record struct CachedImage
{
    /// <summary>
    /// The raw image bytes.
    /// </summary>
    public byte[] Content { get; init; }

    /// <summary>
    /// The MIME type of the image (e.g., "image/png").
    /// </summary>
    public string ContentType { get; init; }
}
