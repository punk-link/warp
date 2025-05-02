namespace Warp.WebApp.Services.Entries;

/// <summary>
/// Defines methods for tracking and retrieving view counts for entries.
/// </summary>
public interface IViewCountService
{
    /// <summary>
    /// Increments the view count for the specified entry and returns the updated count.
    /// </summary>
    /// <param name="itemId">The unique identifier of the entry.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated view count.</returns>
    public Task<long> AddAndGet(Guid itemId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Retrieves the current view count for the specified entry.
    /// </summary>
    /// <param name="itemId">The unique identifier of the entry.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the current view count.</returns>
    public Task<long> Get(Guid itemId, CancellationToken cancellationToken);
}