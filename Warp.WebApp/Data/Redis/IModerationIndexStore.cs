namespace Warp.WebApp.Data.Redis;

/// <summary>
/// Provides sorted-set and lock primitives for scheduling content moderation job processing.
/// </summary>
public interface IModerationIndexStore
{
    /// <summary>
    /// Adds or updates a moderation entry in the index with the provided execution time.
    /// </summary>
    /// <param name="entryId">The identifier of the entry.</param>
    /// <param name="executeAtUtc">The UTC timestamp when the job should be processed.</param>
    /// <param name="cancellationToken">Token used to observe cancellation requests.</param>
    public Task Schedule(Guid entryId, DateTimeOffset executeAtUtc, CancellationToken cancellationToken);

    /// <summary>
    /// Removes the specified entry from the moderation index.
    /// </summary>
    /// <param name="entryId">The identifier of the entry.</param>
    /// <param name="cancellationToken">Token used to observe cancellation requests.</param>
    public Task Remove(Guid entryId, CancellationToken cancellationToken);

    /// <summary>
    /// Removes an entry from the moderation index by its sorted-set member key.
    /// </summary>
    /// <param name="member">The SHA-256 hex member key produced by <see cref="Services.CacheKeyBuilder.BuildModerationMemberKey"/>.</param>
    /// <param name="cancellationToken">Token used to observe cancellation requests.</param>
    public Task Remove(string member, CancellationToken cancellationToken);

    /// <summary>
    /// Returns up to <paramref name="take"/> SHA-256 hex member keys whose scheduled time is at or before <paramref name="maxScoreUtc"/>.
    /// </summary>
    /// <param name="maxScoreUtc">The upper bound for the scheduled time.</param>
    /// <param name="take">Maximum number of members to return.</param>
    /// <param name="cancellationToken">Token used to observe cancellation requests.</param>
    public Task<List<string>> TakeDue(DateTimeOffset maxScoreUtc, int take, CancellationToken cancellationToken);

    /// <summary>
    /// Attempts to acquire an exclusive processing lock for the given entry.
    /// </summary>
    /// <param name="entryId">The identifier of the entry.</param>
    /// <param name="ttl">How long the lock should be held.</param>
    /// <param name="cancellationToken">Token used to observe cancellation requests.</param>
    /// <returns><c>true</c> if the lock was acquired; <c>false</c> if another worker holds it.</returns>
    public Task<bool> TryAcquireLock(Guid entryId, TimeSpan ttl, CancellationToken cancellationToken);

    /// <summary>
    /// Releases the processing lock for the given entry.
    /// </summary>
    /// <param name="entryId">The identifier of the entry.</param>
    /// <param name="cancellationToken">Token used to observe cancellation requests.</param>
    public Task ReleaseLock(Guid entryId, CancellationToken cancellationToken);
}
