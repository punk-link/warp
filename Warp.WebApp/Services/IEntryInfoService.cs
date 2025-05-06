using CSharpFunctionalExtensions;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Creators;
using Warp.WebApp.Models.Errors;

namespace Warp.WebApp.Services;

/// <summary>
/// Service responsible for managing entry information including creation, 
/// retrieval, copying, and removal of entries and their associated images.
/// </summary>
public interface IEntryInfoService
{
    /// <summary>
    /// Adds a new entry to the system for a specific creator.
    /// </summary>
    /// <param name="creator">The creator who is adding the entry.</param>
    /// <param name="entryRequest">The request containing the entry data to be added.</param>
    /// <returns>
    /// A result containing the newly created entry information if successful,
    /// or a domain error if the operation fails.
    /// </returns>
    Task<Result<EntryInfo, DomainError>> Add(Creator creator, EntryRequest entryRequest, CancellationToken cancellationToken);
    
    /// <summary>
    /// Creates a copy of an existing entry for a specific creator.
    /// </summary>
    /// <param name="creator">The creator who is making the copy.</param>
    /// <param name="entryId">The ID of the entry to copy.</param>
    /// <returns>
    /// A result containing the newly copied entry information if successful,
    /// or a domain error if the operation fails.
    /// </returns>
    Task<Result<EntryInfo, DomainError>> Copy(Creator creator, Guid entryId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Retrieves entry information by its ID.
    /// </summary>
    /// <param name="creator">The creator requesting the entry information.</param>
    /// <param name="entryId">The ID of the entry to retrieve.</param>
    /// <returns>
    /// A result containing the requested entry information if successful,
    /// or a domain error if the operation fails.
    /// </returns>
    Task<Result<EntryInfo, DomainError>> Get(Creator creator, Guid entryId, CancellationToken cancellationToken);

    /// <summary>
    /// Removes an entry from the system.
    /// </summary>
    /// <param name="creator">The creator who is removing the entry.</param>
    /// <param name="entryId">The ID of the entry to remove.</param>
    /// <returns>
    /// A unit result indicating success, or a domain error if the operation fails.
    /// </returns>
    Task<UnitResult<DomainError>> Remove(Creator creator, Guid entryId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Removes a specific image from an entry.
    /// </summary>
    /// <param name="creator">The creator who is removing the image.</param>
    /// <param name="entryId">The ID of the entry containing the image.</param>
    /// <param name="imageId">The ID of the image to remove.</param>
    /// <returns>
    /// A unit result indicating success, or a domain error if the operation fails.
    /// </returns>
    Task<UnitResult<DomainError>> RemoveImage(Creator creator, Guid entryId, Guid imageId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Updates an existing entry with new content.
    /// Only the creator can update their entry and only if the view count is 0.
    /// </summary>
    /// <param name="creator">The creator who is updating the entry.</param>
    /// <param name="entryRequest">The request containing the updated entry data.</param>
    /// <returns>
    /// A result containing the updated entry information if successful,
    /// or a domain error if the operation fails.
    /// </returns>
    Task<Result<EntryInfo, DomainError>> Update(Creator creator, EntryRequest entryRequest, CancellationToken cancellationToken);
}