using System.Security.Cryptography;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Options;
using Warp.WebApp.Data;
using Warp.WebApp.Data.Redis;
using Warp.WebApp.Models.Images;
using Warp.WebApp.Models.Options;

namespace Warp.WebApp.Services.Images;

/// <summary>
/// Default implementation of <see cref="IEntryImageLifecycleService"/> that keeps track of entry image metadata and cleanup scheduling.
/// </summary>
public sealed class EntryImageLifecycleService : IEntryImageLifecycleService
{
    public EntryImageLifecycleService(
        IDataStorage dataStorage,
        IEntryLifecycleIndexStore indexStore,
        IOptions<EntryCleanupOptions> options,
        ILogger<EntryImageLifecycleService> logger)
    {
        _dataStorage = dataStorage;
        _indexStore = indexStore;
        _logger = logger;
        _options = options.Value;
        _processingLockTtl = TimeSpan.FromSeconds(_options.ProcessingLockSeconds);
    }


    /// <inheritdoc />
    public async Task<EntryImageLifecycle?> Get(Guid entryId, CancellationToken cancellationToken)
    {
        var key = BuildLifecycleKey(entryId);
        return await _dataStorage.TryGet<EntryImageLifecycle>(key, cancellationToken);
    }


    /// <inheritdoc />
    public async Task Remove(Guid entryId, CancellationToken cancellationToken)
    {
        var lifecycleKey = BuildLifecycleKey(entryId);
        await _dataStorage.Remove<EntryImageLifecycle>(lifecycleKey, cancellationToken);
        await _indexStore.Remove(entryId, cancellationToken);
    }


    /// <inheritdoc />
    public async Task RemoveImage(Guid entryId, Guid imageId, CancellationToken cancellationToken)
    {
        var lifecycle = await Get(entryId, cancellationToken);
        if (lifecycle is null)
            return;

        var filtered = lifecycle.ImageIds.Where(id => id != imageId && id != Guid.Empty).ToArray();
        if (filtered.Length == lifecycle.ImageIds.Count)
            return;

        if (filtered.Length == 0)
        {
            await Remove(entryId, cancellationToken);
            return;
        }

        var updated = lifecycle.WithImages(filtered, lifecycle.ExpiresAt);
        await PersistLifecycle(updated, cancellationToken);
    }


    /// <inheritdoc />
    public async Task Reschedule(EntryImageLifecycle lifecycle, DateTimeOffset resumeAt, CancellationToken cancellationToken)
    {
        var updated = lifecycle.IncrementFailure(resumeAt);
        await PersistLifecycle(updated, cancellationToken);
    }


    /// <inheritdoc />
    public async Task<IReadOnlyList<EntryImageLifecycle>> TakeExpired(DateTimeOffset utcNow, int take, CancellationToken cancellationToken)
    {
        if (take <= 0)
            return [];

        var members = await _indexStore.TakeDue(utcNow, take, cancellationToken);
        if (members.Count == 0)
            return [];

        var lifecycles = new List<EntryImageLifecycle>(members.Count);
        foreach (var member in members)
        {
            var lifecycle = await GetByMember(member, cancellationToken);
            if (lifecycle is null)
            {
                await _indexStore.Remove(member, cancellationToken);
                continue;
            }

            if (lifecycle.ImageIds.Count == 0)
            {
                await Remove(lifecycle.EntryId, cancellationToken);
                continue;
            }

            lifecycles.Add(lifecycle);
        }

        return lifecycles;
    }


    /// <inheritdoc />
    public Task<bool> TryAcquireProcessingLock(Guid entryId, CancellationToken cancellationToken)
        => _indexStore.TryAcquireLock(entryId, _processingLockTtl, cancellationToken);


    /// <inheritdoc />
    public Task ReleaseProcessingLock(Guid entryId, CancellationToken cancellationToken)
        => _indexStore.ReleaseLock(entryId, cancellationToken);


    /// <inheritdoc />
    public async Task Track(Guid entryId, DateTimeOffset expiresAt, IEnumerable<Guid> imageIds, CancellationToken cancellationToken)
    {
        var normalizedImages = imageIds?.Where(id => id != Guid.Empty).ToArray() ?? Array.Empty<Guid>();
        if (normalizedImages.Length == 0)
        {
            await Remove(entryId, cancellationToken);
            return;
        }

        var lifecycle = EntryImageLifecycle.Create(entryId, expiresAt, normalizedImages);
        await PersistLifecycle(lifecycle, cancellationToken);
    }


    private TimeSpan CalculateMetadataTtl(DateTimeOffset expiresAt)
    {
        var buffer = TimeSpan.FromMinutes(Math.Max(1, _options.MetadataRetentionBufferMinutes));
        var ttl = expiresAt - DateTimeOffset.UtcNow + buffer;
        return ttl > buffer ? ttl : buffer;
    }


    private async Task PersistLifecycle(EntryImageLifecycle lifecycle, CancellationToken cancellationToken)
    {
        var ttl = CalculateMetadataTtl(lifecycle.ExpiresAt);
        var key = BuildLifecycleKey(lifecycle.EntryId);

        var setResult = await _dataStorage.Set(key, lifecycle, ttl, cancellationToken);
        if (setResult.IsFailure)
        {
            _logger.LogWarning("Failed to persist lifecycle metadata for entry {EntryId}: {Reason}", lifecycle.EntryId, setResult.Error.Detail);
            return;
        }

        await _indexStore.Schedule(lifecycle.EntryId, lifecycle.ExpiresAt, cancellationToken);
    }


    private static string BuildLifecycleKey(Guid entryId)
        => CacheKeyBuilder.BuildEntryImageLifecycleKey(NormalizeEntryId(entryId));


    private static string NormalizeEntryId(Guid entryId)
    {
        Span<byte> entryBytes = stackalloc byte[16];
        entryId.TryWriteBytes(entryBytes);

        Span<byte> hashBytes = stackalloc byte[32];
        SHA256.HashData(entryBytes, hashBytes);

        return Convert.ToHexString(hashBytes);
    }


    private ValueTask<EntryImageLifecycle?> GetByMember(string member, CancellationToken cancellationToken)
    {
        var key = CacheKeyBuilder.BuildEntryImageLifecycleKey(member);
        return _dataStorage.TryGet<EntryImageLifecycle>(key, cancellationToken);
    }


    private readonly IDataStorage _dataStorage;
    private readonly IEntryLifecycleIndexStore _indexStore;
    private readonly ILogger<EntryImageLifecycleService> _logger;
    private readonly EntryCleanupOptions _options;
    private readonly TimeSpan _processingLockTtl;
}
