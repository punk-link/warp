using Warp.WebApp.Models.Entries;
using Warp.WebApp.Models.Moderation;

namespace Warp.WebApp.Services.Moderation;

/// <summary>
/// Manages the lifecycle of content moderation jobs, including scheduling,
/// retrieval, persistence, and retry logic.
/// </summary>
public interface IModerationJobService
{
    /// <summary>
    /// Retrieves a moderation job by its index member key.
    /// </summary>
    /// <param name="member">The member key used in the index (N-formatted GUID from <see cref="Warp.WebApp.Services.CacheKeyBuilder.BuildModerationMemberKey"/>).</param>
    /// <param name="cancellationToken">Token used to observe cancellation requests.</param>
    public ValueTask<EntryModerationJob?> GetByMember(string member, CancellationToken cancellationToken);

    /// <summary>
    /// Persists the current state of a moderation job.
    /// </summary>
    /// <param name="job">The job to persist.</param>
    /// <param name="cancellationToken">Token used to observe cancellation requests.</param>
    public Task Persist(EntryModerationJob job, CancellationToken cancellationToken);

    /// <summary>
    /// Releases the processing lock for the given entry.
    /// </summary>
    /// <param name="entryId">The identifier of the entry.</param>
    /// <param name="cancellationToken">Token used to observe cancellation requests.</param>
    public Task ReleaseProcessingLock(Guid entryId, CancellationToken cancellationToken);

    /// <summary>
    /// Removes a completed moderation job from the store and index.
    /// </summary>
    /// <param name="job">The completed job to remove.</param>
    /// <param name="cancellationToken">Token used to observe cancellation requests.</param>
    public Task Remove(EntryModerationJob job, CancellationToken cancellationToken);

    /// <summary>
    /// Removes a member directly from the index without loading the job first.
    /// Used when the backing job record is missing.
    /// </summary>
    /// <param name="member">The member key used in the index (N-formatted GUID from <see cref="Warp.WebApp.Services.CacheKeyBuilder.BuildModerationMemberKey"/>).</param>
    /// <param name="cancellationToken">Token used to observe cancellation requests.</param>
    public Task RemoveMember(string member, CancellationToken cancellationToken);

    /// <summary>
    /// Reschedules a job for a future retry with an incremented failure counter.
    /// </summary>
    /// <param name="job">The job to reschedule.</param>
    /// <param name="retryAt">The UTC time at which the job should next be attempted.</param>
    /// <param name="cancellationToken">Token used to observe cancellation requests.</param>
    public Task Reschedule(EntryModerationJob job, DateTimeOffset retryAt, CancellationToken cancellationToken);

    /// <summary>
    /// Schedules a new moderation job for the given entry.
    /// If neither text nor images need moderation the call is a no-op.
    /// </summary>
    /// <param name="entryInfo">The entry that was just saved.</param>
    /// <param name="cancellationToken">Token used to observe cancellation requests.</param>
    public Task Schedule(EntryInfo entryInfo, CancellationToken cancellationToken);

    /// <summary>
    /// Returns due moderation jobs from the index, up to <paramref name="take"/> items.
    /// </summary>
    /// <param name="utcNow">The current UTC time used as the upper score bound.</param>
    /// <param name="take">Maximum number of jobs to dequeue.</param>
    /// <param name="cancellationToken">Token used to observe cancellation requests.</param>
    public Task<List<string>> TakeDue(DateTimeOffset utcNow, int take, CancellationToken cancellationToken);

    /// <summary>
    /// Attempts to acquire an exclusive processing lock for the given entry.
    /// </summary>
    /// <param name="entryId">The identifier of the entry.</param>
    /// <param name="cancellationToken">Token used to observe cancellation requests.</param>
    /// <returns><c>true</c> if the lock was acquired; otherwise <c>false</c>.</returns>
    public Task<bool> TryAcquireProcessingLock(Guid entryId, CancellationToken cancellationToken);
}
