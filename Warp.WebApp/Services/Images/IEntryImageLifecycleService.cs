using Warp.WebApp.Models.Images;

namespace Warp.WebApp.Services.Images;

/// <summary>
/// Provides operations to track and manage entry image lifecycle metadata for cleanup routines.
/// </summary>
public interface IEntryImageLifecycleService
{
    /// <summary>
    /// Gets the lifecycle metadata for the specified entry ID.
    /// </summary>
    /// <param name="entryId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<EntryImageLifecycle?> Get(Guid entryId, CancellationToken cancellationToken);

    /// <summary>
    /// Removes the lifecycle metadata for the specified entry ID.
    /// </summary>
    /// <param name="entryId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task Remove(Guid entryId, CancellationToken cancellationToken);

    /// <summary>
    /// Removes the specified image ID from the lifecycle metadata of the given entry ID.
    /// </summary>
    /// <param name="entryId"></param>
    /// <param name="imageId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task RemoveImage(Guid entryId, Guid imageId, CancellationToken cancellationToken);

    /// <summary>
    /// Reschedules the lifecycle processing for the given entry image lifecycle metadata.
    /// </summary>
    /// <param name="lifecycle"></param>
    /// <param name="resumeAt"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task Reschedule(EntryImageLifecycle lifecycle, DateTimeOffset resumeAt, CancellationToken cancellationToken);

    /// <summary>
    /// Takes expired entry image lifecycles up to the specified count.
    /// </summary>
    /// <param name="utcNow"></param>
    /// <param name="take"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<IReadOnlyList<EntryImageLifecycle>> TakeExpired(DateTimeOffset utcNow, int take, CancellationToken cancellationToken);

    /// <summary>
    /// Attempts to acquire a processing lock for the specified entry ID.
    /// </summary>
    /// <param name="entryId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<bool> TryAcquireProcessingLock(Guid entryId, CancellationToken cancellationToken);

    /// <summary>
    /// Releases a previously acquired processing lock for the specified entry ID.
    /// </summary>
    /// <param name="entryId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task ReleaseProcessingLock(Guid entryId, CancellationToken cancellationToken);

    /// <summary>
    /// Tracks a new entry image lifecycle for the specified entry ID.
    /// </summary>
    /// <param name="entryId"></param>
    /// <param name="expiresAt"></param>
    /// <param name="imageIds"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task Track(Guid entryId, DateTimeOffset expiresAt, IEnumerable<Guid> imageIds, CancellationToken cancellationToken);
}
