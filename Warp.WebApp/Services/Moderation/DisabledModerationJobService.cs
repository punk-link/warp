using Warp.WebApp.Models.Entries;
using Warp.WebApp.Models.Moderation;

namespace Warp.WebApp.Services.Moderation;

/// <summary>
/// No-op moderation job service used when content moderation is disabled.
/// </summary>
public sealed class DisabledModerationJobService : IModerationJobService
{
    /// <inheritdoc />
    public ValueTask<EntryModerationJob?> GetByMember(string member, CancellationToken cancellationToken)
        => ValueTask.FromResult<EntryModerationJob?>(null);


    /// <inheritdoc />
    public Task Persist(EntryModerationJob job, CancellationToken cancellationToken)
        => Task.CompletedTask;


    /// <inheritdoc />
    public Task ReleaseProcessingLock(Guid entryId, CancellationToken cancellationToken)
        => Task.CompletedTask;


    /// <inheritdoc />
    public Task Remove(EntryModerationJob job, CancellationToken cancellationToken)
        => Task.CompletedTask;


    /// <inheritdoc />
    public Task RemoveMember(string member, CancellationToken cancellationToken)
        => Task.CompletedTask;


    /// <inheritdoc />
    public Task Reschedule(EntryModerationJob job, DateTimeOffset retryAt, CancellationToken cancellationToken)
        => Task.CompletedTask;


    /// <inheritdoc />
    public Task Schedule(EntryInfo entryInfo, CancellationToken cancellationToken)
        => Task.CompletedTask;


    /// <inheritdoc />
    public Task<List<string>> TakeDue(DateTimeOffset utcNow, int take, CancellationToken cancellationToken)
        => Task.FromResult(new List<string>());


    /// <inheritdoc />
    public Task<bool> TryAcquireProcessingLock(Guid entryId, CancellationToken cancellationToken)
        => Task.FromResult(false);
}
