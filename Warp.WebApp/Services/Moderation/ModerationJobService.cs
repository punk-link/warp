using Microsoft.Extensions.Options;
using Warp.WebApp.Data;
using Warp.WebApp.Data.Redis;
using Warp.WebApp.Models.Entries;
using Warp.WebApp.Models.Moderation;
using Warp.WebApp.Models.Options;

namespace Warp.WebApp.Services.Moderation;

/// <summary>
/// Default implementation of <see cref="IModerationJobService"/> that persists
/// moderation jobs in KeyDB and schedules them via a sorted-set index.
/// </summary>
public sealed class ModerationJobService : IModerationJobService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModerationJobService"/> class.
    /// </summary>
    /// <param name="dataStorage">The data storage provider.</param>
    /// <param name="indexStore">The sorted-set index store for scheduled moderation jobs.</param>
    /// <param name="options">Moderation configuration options.</param>
    public ModerationJobService(IDataStorage dataStorage, IModerationIndexStore indexStore, IOptions<ContentModerationOptions> options)
    {
        _dataStorage = dataStorage;
        _indexStore = indexStore;
        _options = options.Value;

        _processingLockTtl = TimeSpan.FromSeconds(_options.ProcessingLockSeconds);
    }


    /// <inheritdoc />
    public ValueTask<EntryModerationJob?> GetByMember(string member, CancellationToken cancellationToken)
    {
        var key = CacheKeyBuilder.BuildModerationJobKey(member);
        return _dataStorage.TryGet<EntryModerationJob>(key, cancellationToken);
    }


    /// <inheritdoc />
    public Task Persist(EntryModerationJob job, CancellationToken cancellationToken)
        => PersistInternal(job, cancellationToken);


    /// <inheritdoc />
    public Task ReleaseProcessingLock(Guid entryId, CancellationToken cancellationToken)
        => _indexStore.ReleaseLock(entryId, cancellationToken);


    /// <inheritdoc />
    public async Task Remove(EntryModerationJob job, CancellationToken cancellationToken)
    {
        var member = NormalizeMember(job.EntryId);
        var key = CacheKeyBuilder.BuildModerationJobKey(member);

        await _dataStorage.Remove<EntryModerationJob>(key, cancellationToken);
        await _indexStore.Remove(member, cancellationToken);
    }


    /// <inheritdoc />
    public Task RemoveMember(string member, CancellationToken cancellationToken)
        => _indexStore.Remove(member, cancellationToken);


    /// <inheritdoc />
    public async Task Reschedule(EntryModerationJob job, DateTimeOffset retryAt, CancellationToken cancellationToken)
    {
        var updated = job.IncrementFailure(retryAt);
        await PersistInternal(updated, cancellationToken);
    }


    /// <inheritdoc />
    public async Task Schedule(EntryInfo entryInfo, CancellationToken cancellationToken)
    {
        var hasText = !string.IsNullOrWhiteSpace(entryInfo.Entry.Content);
        var imageIds = entryInfo.ImageInfos.Select(info => info.Id)
            .ToList();

        if (!hasText && imageIds.Count == 0)
            return;

        var now = DateTimeOffset.UtcNow;
        var job = EntryModerationJob.Create(entryInfo.Id, hasText, imageIds, entryInfo.ExpiresAt, now);
        await PersistInternal(job, cancellationToken);
    }


    /// <inheritdoc />
    public Task<List<string>> TakeDue(DateTimeOffset utcNow, int take, CancellationToken cancellationToken)
        => _indexStore.TakeDue(utcNow, take, cancellationToken);


    /// <inheritdoc />
    public Task<bool> TryAcquireProcessingLock(Guid entryId, CancellationToken cancellationToken)
        => _indexStore.TryAcquireLock(entryId, _processingLockTtl, cancellationToken);


    private TimeSpan CalculateTtl(DateTimeOffset expiresAt)
    {
        var buffer = TimeSpan.FromMinutes(Math.Max(1, _options.MetadataRetentionBufferMinutes));
        var ttl = expiresAt - DateTimeOffset.UtcNow + buffer;
        return ttl > buffer ? ttl : buffer;
    }


    private static string NormalizeMember(in Guid entryId)
        => CacheKeyBuilder.BuildModerationMemberKey(in entryId);


    private async Task PersistInternal(EntryModerationJob job, CancellationToken cancellationToken)
    {
        var ttl = CalculateTtl(job.ExpiresAt);
        var member = NormalizeMember(job.EntryId);
        var key = CacheKeyBuilder.BuildModerationJobKey(member);

        await _dataStorage.Set(key, job, ttl, cancellationToken);
        await _indexStore.Schedule(job.EntryId, job.NextAttemptAt, cancellationToken);
    }


    private readonly IDataStorage _dataStorage;
    private readonly IModerationIndexStore _indexStore;
    private readonly ContentModerationOptions _options;
    private readonly TimeSpan _processingLockTtl;
}
