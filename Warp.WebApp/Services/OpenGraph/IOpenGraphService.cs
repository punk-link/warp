using CSharpFunctionalExtensions;
using Warp.WebApp.Models.Entries;
using Warp.WebApp.Models.Errors;

namespace Warp.WebApp.Services.OpenGraph;

public interface IOpenGraphService
{
    /// <summary>
    /// Adds an OpenGraph description for the specified entry.
    /// </summary>
    /// <param name="entryId"> The ID of the entry to which the OpenGraph description will be added.</param>
    /// <param name="openGraphDescription"> The OpenGraph description to be added.</param>
    /// <param name="expiresIn"> The duration after which the OpenGraph description will expire.</param>
    /// <param name="cancellationToken"> A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// Returns a <see cref="UnitResult{T}"/> containing a <see cref="DomainError"/> if the operation fails, or an empty result if it succeeds.
    /// </returns>
    Task<UnitResult<DomainError>> Add(Guid entryId, EntryOpenGraphDescription openGraphDescription, TimeSpan expiresIn, CancellationToken cancellationToken);

    /// <summary>
    /// Adds an OpenGraph description for the specified entry.
    /// </summary>
    /// <param name="entryId"> The ID of the entry to which the OpenGraph description will be added.</param>
    /// <param name="descriptionSource"> The source text for the OpenGraph description.</param>
    /// <param name="previewImageUri"> An optional URI for the preview image associated with the OpenGraph description.</param>
    /// <param name="expiresIn"> The duration after which the OpenGraph description will expire.</param>
    /// <param name="cancellationToken"> A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// Returns a <see cref="UnitResult{T}"/> containing a <see cref="DomainError"/> if the operation fails, or an empty result if it succeeds.
    /// </returns>
    Task<UnitResult<DomainError>> Add(Guid entryId, string descriptionSource, Uri? previewImageUri, TimeSpan expiresIn, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the OpenGraph description for the specified entry.
    /// </summary>
    /// <param name="entryId"> The ID of the entry for which the OpenGraph description is requested.</param>
    /// <param name="cancellationToken"> A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// Returns an <see cref="EntryOpenGraphDescription"/> containing the OpenGraph description for the entry.
    /// </returns>
    Task<EntryOpenGraphDescription> Get(Guid entryId, CancellationToken cancellationToken);

    EntryOpenGraphDescription BuildDescription(string descriptionSource, Uri? previewImageUrl);
    
    EntryOpenGraphDescription GetDefaultDescription();
}