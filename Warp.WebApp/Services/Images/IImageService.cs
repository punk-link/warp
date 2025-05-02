using CSharpFunctionalExtensions;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Errors;

namespace Warp.WebApp.Services.Images;

public interface IImageService
{
    /// <summary>
    /// Retrieves metadata for a list of images associated with a specific entry.
    /// </summary>
    /// <param name="entryId">The ID of the entry containing the images.</param>
    /// <param name="imageIds">The list of image IDs to retrieve metadata for.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>
    /// A result containing a list of image metadata if successful, 
    /// or an error if the operation fails.
    /// </returns>
    public Task<Result<List<ImageInfo>, DomainError>> GetAttached(Guid entryId, List<Guid> imageIds, CancellationToken cancellationToken);

    /// <summary>
    /// Copies all images from one entry to another.
    /// </summary>
    /// <param name="sourceEntryId">The ID of the entry containing the source images.</param>
    /// <param name="targetEntryId">The ID of the entry to copy the images to.</param>
    /// <param name="sourceImages">The list of image infos to copy.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>
    /// A result containing a list of new image information if successful,
    /// or an error if the operation fails.
    /// </returns>
    public Task<Result<List<ImageInfo>, DomainError>> Copy(Guid sourceEntryId, Guid targetEntryId, List<ImageInfo> sourceImages, CancellationToken cancellationToken);

    /// <summary>
    /// Removes an image from storage.
    /// </summary>
    /// <param name="entryId">The ID of the entry that owns the image.</param>
    /// <param name="imageId">The ID of the image to remove.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>
    /// A unit result indicating success, or an error if the operation fails.
    /// </returns>
    public Task<UnitResult<DomainError>> Remove(Guid entryId, Guid imageId, CancellationToken cancellationToken);
}
