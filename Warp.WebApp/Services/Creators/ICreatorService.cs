using CSharpFunctionalExtensions;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Creators;
using Warp.WebApp.Models.Errors;

namespace Warp.WebApp.Services.Creators;

/// <summary>
/// Provides functionality for managing creators in the application.
/// </summary>
public interface ICreatorService
{
    /// <summary>
    /// Creates a new creator with a randomly generated ID.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, with the newly created <see cref="Creator"/> instance.</returns>
    public Task<Creator> Add(CancellationToken cancellationToken);
    
    /// <summary>
    /// Attaches an entry to a creator, storing the association in the data storage.
    /// </summary>
    /// <param name="creator">The creator to attach the entry to.</param>
    /// <param name="entryInfo">The entry information to attach.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, with a result indicating success or a domain error.</returns>
    public Task<UnitResult<DomainError>> AttachEntry(Creator creator, EntryInfo entryInfo, CancellationToken cancellationToken);
    
    /// <summary>
    /// Retrieves a creator by their ID.
    /// </summary>
    /// <param name="creatorId">The ID of the creator to retrieve. Can be null.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, with a result containing either the creator or a domain error.</returns>
    public Task<Result<Creator, DomainError>> Get(Guid? creatorId, CancellationToken cancellationToken);
}