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

        var redisTransaction = db.CreateTransaction();
        var redisTaskPush = redisTransaction.ListRightPushAsync(key, bytes);
        await ExecuteOrCancel(redisTaskPush, cancellationToken);
        var redisTaskSetExpirationTime = redisTransaction.KeyExpireTimeAsync(key);
        await ExecuteOrCancel(redisTaskSetExpirationTime, cancellationToken);
        await redisTransaction.ExecuteAsync(CommandFlags.FireAndForget);

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
            _ => 0
        };


    private readonly IConnectionMultiplexer _multiplexer;
}