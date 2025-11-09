namespace Warp.WebApp.Data.Redis;

/// <summary>
/// Provides sorted-set and lock primitives for scheduling entry image lifecycle processing.
/// </summary>
public interface IEntryLifecycleIndexStore
{
    /// <summary>
    /// Adds or updates an entry in the lifecycle index with the provided execution time.
    /// </summary>
    /// <param name="entryId">The identifier of the entry.</param>
    /// <param name="executeAtUtc">The UTC timestamp when the entry should be processed.</param>
    /// <param name="cancellationToken">Token used to observe cancellation requests.</param>
    public Task Schedule(Guid entryId, DateTime executeAtUtc, CancellationToken cancellationToken);

    /// <summary>
    /// Removes the specified entry from the lifecycle index.
    /// </summary>
    /// <param name="entryId">The identifier of the entry.</param>
    /// <param name="cancellationToken">Token used to observe cancellation requests.</param>
    public Task Remove(Guid entryId, CancellationToken cancellationToken);

    /// <summary>
    /// Removes the provided index member.
    /// </summary>
    /// <param name="member">The raw sorted-set member value.</param>
    /// <param name="cancellationToken">Token used to observe cancellation requests.</param>
    public Task Remove(string member, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves lifecycle members whose score is less than or equal to the specified timestamp.
    /// </summary>
    /// <param name="maxScoreUtc">The inclusive maximum UTC timestamp.</param>
    /// <param name="take">The maximum number of members to return.</param>
    /// <param name="cancellationToken">Token used to observe cancellation requests.</param>
    /// <returns>A read-only list of lifecycle members.</returns>
    public Task<IReadOnlyList<string>> TakeDue(DateTime maxScoreUtc, int take, CancellationToken cancellationToken);

    /// <summary>
    /// Attempts to acquire a processing lock for the specified entry.
    /// </summary>
    /// <param name="entryId">The identifier of the entry.</param>
    /// <param name="ttl">The lock time-to-live.</param>
    /// <param name="cancellationToken">Token used to observe cancellation requests.</param>
    /// <returns><c>true</c> if the lock was acquired; otherwise, <c>false</c>.</returns>
    public Task<bool> TryAcquireLock(Guid entryId, TimeSpan ttl, CancellationToken cancellationToken);

    /// <summary>
    /// Releases a previously acquired processing lock for the specified entry.
    /// </summary>
    /// <param name="entryId">The identifier of the entry.</param>
    /// <param name="cancellationToken">Token used to observe cancellation requests.</param>
    public Task ReleaseLock(Guid entryId, CancellationToken cancellationToken);
}
