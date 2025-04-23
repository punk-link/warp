using CSharpFunctionalExtensions;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Errors;

namespace Warp.WebApp.Services.Entries;

/// <summary>
/// Provides functionality for managing entry content in the application.
/// </summary>
public interface IEntryService
{
    /// <summary>
    /// Creates a new entry with formatted content from the provided request.
    /// </summary>
    /// <param name="entryRequest">The request containing the entry content and metadata.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task representing the asynchronous operation, with a result containing either 
    /// the successfully created entry or a domain error if validation fails.
    /// </returns>
    public Task<Result<Entry, DomainError>> Add(EntryRequest entryRequest, CancellationToken cancellationToken);
}