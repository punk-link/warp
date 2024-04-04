using System.Text.Json;
using StackExchange.Redis;
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
        var redisTaskValue = db.StringIncrementAsync(key);
        var completedTask = await Task.WhenAny(redisTaskValue, Task.Delay(Timeout.Infinite, cancellationToken));
        if (completedTask == redisTaskValue)
            return await redisTaskValue;
        
        cancellationToken.ThrowIfCancellationRequested();
        return default;
    }


    public Task<bool> Contains<T>(string key)
    {
        var db = GetDatabase<T>();
        return db.KeyExistsAsync(key);
    }


    public void Remove<T>(string key)
    {
        var db = GetDatabase<T>();
        db.StringGetDelete(key, CommandFlags.FireAndForget);
    }


    public async Task Set<T>(string key, T value, TimeSpan expiresIn, CancellationToken cancellationToken)
    {
        var db = GetDatabase<T>();
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
        var redisTask = db.StringSetAsync(key, bytes, expiresIn);
        await Task.WhenAny(redisTask, Task.Delay(Timeout.Infinite, cancellationToken));
        cancellationToken.ThrowIfCancellationRequested();
    }


    public async Task<T?> TryGet<T>(string key, CancellationToken cancellationToken)
    {
        var db = GetDatabase<T>();
        var redisValueTask = db.StringGetAsync(key);

        var completedTask = await Task.WhenAny(redisValueTask, Task.Delay(Timeout.Infinite, cancellationToken));
        if (completedTask == redisValueTask)
        {
            var redisValue = await redisValueTask;
            if (!redisValue.HasValue)
                return default;

            var bytes = (byte[])redisValue!;
            return JsonSerializer.Deserialize<T>(bytes)!;
        }

        cancellationToken.ThrowIfCancellationRequested();
        return default;
    }


    private IDatabase GetDatabase<T>()
    {
        var dbIndex = ToDatabaseIndex(typeof(T));
        return _multiplexer.GetDatabase(dbIndex);
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