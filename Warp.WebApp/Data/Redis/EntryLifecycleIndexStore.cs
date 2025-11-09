using StackExchange.Redis;
using System.Security.Cryptography;
using Warp.WebApp.Models.Images;
using Warp.WebApp.Services;

namespace Warp.WebApp.Data.Redis;

/// <summary>
/// Default implementation of <see cref="IEntryLifecycleIndexStore"/> backed by KeyDB sorted sets and locks.
/// </summary>
public sealed class EntryLifecycleIndexStore : RedisStoreBase, IEntryLifecycleIndexStore
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntryLifecycleIndexStore"/> class.
    /// </summary>
    /// <param name="multiplexer">The Redis connection multiplexer.</param>
    public EntryLifecycleIndexStore(IConnectionMultiplexer multiplexer) : base(multiplexer) { }


    /// <inheritdoc />
    public async Task Schedule(Guid entryId, DateTime executeAtUtc, CancellationToken cancellationToken)
    {
        var member = NormalizeMember(entryId);
        var score = ToScore(executeAtUtc);
        var task = GetDatabase<string>()
            .SortedSetAddAsync(IndexKey, member, score);

        await ExecuteOrCancel(task, cancellationToken);
    }


    /// <inheritdoc />
    public async Task Remove(Guid entryId, CancellationToken cancellationToken)
    {
        var member = NormalizeMember(entryId);
        await Remove(member, cancellationToken);
    }


    /// <inheritdoc />
    public async Task Remove(string member, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(member))
            return;

        var task = GetDatabase<string>()
            .SortedSetRemoveAsync(IndexKey, member);
        await ExecuteOrCancel(task, cancellationToken);
    }


    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> TakeDue(DateTime maxScoreUtc, int take, CancellationToken cancellationToken)
    {
        if (take <= 0)
            return [];

        var task = GetDatabase<string>()
            .SortedSetRangeByScoreAsync(IndexKey, double.NegativeInfinity, ToScore(maxScoreUtc), Exclude.None, Order.Ascending, 0, take);
        var members = await ExecuteOrCancel(task, cancellationToken);
        if (members is null || members.Length == 0)
            return [];

        var results = new List<string>(members.Length);
        foreach (var member in members)
        {
            if (!member.HasValue)
                continue;

            results.Add(member.ToString());
        }

        return results;
    }


    /// <inheritdoc />
    public async Task<bool> TryAcquireLock(Guid entryId, TimeSpan ttl, CancellationToken cancellationToken)
    {
        var member = NormalizeMember(entryId);
        var task = GetDatabase<string>()
            .StringSetAsync(CacheKeyBuilder.BuildLockKey(member), "1", ttl, When.NotExists);

        return await ExecuteOrCancel(task, cancellationToken);
    }


    /// <inheritdoc />
    public async Task ReleaseLock(Guid entryId, CancellationToken cancellationToken)
    {
        var member = NormalizeMember(entryId);
        var task = GetDatabase<string>()
            .KeyDeleteAsync(CacheKeyBuilder.BuildLockKey(member));

        await ExecuteOrCancel(task, cancellationToken);
    }


    private static string NormalizeMember(in Guid entryId)
    {
        Span<byte> entryBytes = stackalloc byte[16];
        entryId.TryWriteBytes(entryBytes);

        Span<byte> hashBytes = stackalloc byte[32];
        SHA256.HashData(entryBytes, hashBytes);

        return Convert.ToHexString(hashBytes);
    }


    private static double ToScore(DateTime timestampUtc)
        => new DateTimeOffset(timestampUtc, TimeSpan.Zero).ToUnixTimeMilliseconds();


    private const string IndexKey = nameof(EntryImageLifecycle);
}
