using Microsoft.AspNetCore.DataProtection.KeyManagement;
using StackExchange.Redis;
using System.Text.Json;
using Warp.WebApp.Models;

namespace Warp.WebApp.Data.Redis;

public sealed class KeyDbStorage : IDistributedStorage
{
    public KeyDbStorage(IConnectionMultiplexer multiplexer)
    {
        _multiplexer = multiplexer;
    }


    public async Task<long> AddAndGetCounter(string key, CancellationToken cancellationToken)
    {
        var db = GetDatabase<object>();
        var redisTask = db.StringIncrementAsync(key);
        return await ExecuteOrCancel(redisTask, cancellationToken);
    }


    public async Task<bool> Contains<T>(string key, CancellationToken cancellationToken)
    {
        var db = GetDatabase<T>();
        var redisTask = db.KeyExistsAsync(key);
        return await ExecuteOrCancel(redisTask, cancellationToken);
    }

    public async Task Remove<T>(string key, CancellationToken cancellationToken)
    {
        var db = GetDatabase<T>();
        var redisTask = db.StringGetDeleteAsync(key);
        await ExecuteOrCancel(redisTask, cancellationToken);
    }


    public async Task Set<T>(string key, T value, TimeSpan expiresIn, CancellationToken cancellationToken)
    {
        var db = GetDatabase<T>();
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
        var redisTask = db.StringSetAsync(key, bytes, expiresIn);
        await ExecuteOrCancel(redisTask, cancellationToken);
    }


    public async Task CrossValueSet<K, V>(string keyK, K valueK, TimeSpan expiresInK, string keyV, V valueV, TimeSpan expiresInV, CancellationToken cancellationToken)
    {
        var db = _multiplexer.GetDatabase();
        var bytesK = JsonSerializer.SerializeToUtf8Bytes(valueK);
        var bytesV = JsonSerializer.SerializeToUtf8Bytes(valueV);
        var redisTransaction = db.CreateTransaction();

        var transactionalTask = Task.WhenAll(redisTransaction.SetAddAsync(keyK, bytesK), redisTransaction.KeyExpireAsync(keyK, expiresInK),
            redisTransaction.StringSetAsync(keyV, bytesV, expiresInV));

        var isExecuted = await ExecuteOrCancel(redisTransaction.ExecuteAsync(), cancellationToken);
        if (isExecuted)
            await transactionalTask;
    }


    public async Task<HashSet<T>> TryGetSet<T>(string key, CancellationToken cancellationToken)
    {
        var db = GetDatabase<T>();
        var redisTask = db.SetMembersAsync(key);

        var completedTask = await ExecuteOrCancel(redisTask!, cancellationToken);

        return completedTask.Select(d => JsonSerializer.Deserialize<T>(d)).ToHashSet();
    }


    public async Task<T?> TryGet<T>(string key, CancellationToken cancellationToken)
    {
        var db = GetDatabase<T>();
        var redisTask = db.StringGetAsync(key);

        var completedTask = await ExecuteOrCancel(redisTask, cancellationToken);

        if (!completedTask.HasValue)
            return default;

        var bytes = (byte[])completedTask!;
        return JsonSerializer.Deserialize<T>(bytes)!;
    }


    public async Task<bool> IsValueContainsInSet<T>(string key, T value, CancellationToken cancellationToken)
    {
        var db = GetDatabase<T>();
        var valueBytes = JsonSerializer.SerializeToUtf8Bytes(value);
        var redisTask = db.SetContainsAsync(key, valueBytes);

        return await ExecuteOrCancel(redisTask, cancellationToken);
    }


    private IDatabase GetDatabase<T>()
    {
        var dbIndex = ToDatabaseIndex(typeof(T));
        return _multiplexer.GetDatabase(dbIndex);
    }

    private async Task<T?> ExecuteOrCancel<T>(Task<T?> task, CancellationToken cancellationToken)
    {
        var completedTask = await Task.WhenAny(task, Task.Delay(Timeout.Infinite, cancellationToken));
        if (completedTask == task)
            return await task;

        cancellationToken.ThrowIfCancellationRequested();
        return default;
    }

    private static int ToDatabaseIndex<T>(T type)
        => type switch
        {
            Entry => 1,
            ImageInfo => 2,
            Report => 3,
            string => 4,
            Guid => 5,
            _ => 0
        };

    private readonly IConnectionMultiplexer _multiplexer;
}