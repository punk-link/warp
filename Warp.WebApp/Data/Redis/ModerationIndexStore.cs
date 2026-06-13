using StackExchange.Redis;
using Warp.WebApp.Models.Moderation;
using Warp.WebApp.Services;

namespace Warp.WebApp.Data.Redis;

/// <summary>
/// Default implementation of <see cref="IModerationIndexStore"/> backed by a KeyDB sorted set and string locks.
/// </summary>
public sealed class ModerationIndexStore : RedisStoreBase, IModerationIndexStore
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModerationIndexStore"/> class.
    /// </summary>
    /// <param name="multiplexer">The Redis connection multiplexer.</param>
    public ModerationIndexStore(IConnectionMultiplexer multiplexer) : base(multiplexer) { }


    /// <inheritdoc />
    public async Task Schedule(Guid entryId, DateTimeOffset executeAtUtc, CancellationToken cancellationToken)
    {
        var member = NormalizeMember(entryId);
        var score = ToScore(executeAtUtc);
        var task = GetDatabase<EntryModerationJob>()
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

        var task = GetDatabase<EntryModerationJob>()
            .SortedSetRemoveAsync(IndexKey, member);

        await ExecuteOrCancel(task, cancellationToken);
    }


    /// <inheritdoc />
    public async Task<List<string>> TakeDue(DateTimeOffset maxScoreUtc, int take, CancellationToken cancellationToken)
    {
        if (take <= 0)
            return [];

        var task = GetDatabase<EntryModerationJob>()
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
        var task = GetDatabase<EntryModerationJob>()
            .StringSetAsync(CacheKeyBuilder.BuildModerationLockKey(member), "1", ttl, When.NotExists);

        return await ExecuteOrCancel(task, cancellationToken);
    }


    /// <inheritdoc />
    public async Task ReleaseLock(Guid entryId, CancellationToken cancellationToken)
    {
        var member = NormalizeMember(entryId);
        var task = GetDatabase<EntryModerationJob>()
            .KeyDeleteAsync(CacheKeyBuilder.BuildModerationLockKey(member));

        await ExecuteOrCancel(task, cancellationToken);
    }


    private static string NormalizeMember(in Guid entryId)
        => CacheKeyBuilder.BuildModerationMemberKey(entryId);


    private static double ToScore(DateTimeOffset timestampUtc)
        => timestampUtc.ToUnixTimeMilliseconds();


    private const string IndexKey = nameof(EntryModerationJob);
}
