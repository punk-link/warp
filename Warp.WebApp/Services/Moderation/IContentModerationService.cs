using Warp.WebApp.Models.Moderation;

namespace Warp.WebApp.Services.Moderation;

/// <summary>
/// Provides content moderation for text and images via an external provider.
/// </summary>
public interface IContentModerationService
{
    /// <summary>
    /// Submits plain text to the moderation provider and returns the result.
    /// </summary>
    /// <param name="plainText">The text to moderate.</param>
    /// <param name="cancellationToken">Token used to observe cancellation requests.</param>
    public Task<ModerationResult> ModerateText(string plainText, CancellationToken cancellationToken);

    /// <summary>
    /// Submits image bytes to the moderation provider and returns the result.
    /// </summary>
    /// <param name="imageBytes">The raw image bytes.</param>
    /// <param name="contentType">The MIME type of the image (e.g. <c>image/jpeg</c>).</param>
    /// <param name="cancellationToken">Token used to observe cancellation requests.</param>
    public Task<ModerationResult> ModerateImage(byte[] imageBytes, string contentType, CancellationToken cancellationToken);
}
