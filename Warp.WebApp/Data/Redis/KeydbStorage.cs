using System.Text.Json;
using StackExchange.Redis;
using Warp.WebApp.Models;

namespace Warp.WebApp.Data.Redis;

public class KeyDbStorage : IDistributedStorage
{
    public KeyDbStorage(IConnectionMultiplexer multiplexer)
    {
        _multiplexer = multiplexer;
    }


    public Task<long> AddAndGetCounter(string key)
    {
        var db = GetDatabase<object>();
        return db.StringIncrementAsync(key);
    }


    public void Remove<T>(string key)
    {
        var db = GetDatabase<T>();
        db.StringGetDelete(key, CommandFlags.FireAndForget);
    }


    public async Task Set<T>(string key, T value, TimeSpan expiresIn)
    {
        var db = GetDatabase<T>();
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
        await db.StringSetAsync(key, bytes, expiresIn);
    }


    public async Task<T?> TryGet<T>(string key)
    {
        var db = GetDatabase<T>();
        var redisValue = await db.StringGetAsync(key);
        if (!redisValue.HasValue)
            return default;

        var bytes = (byte[])redisValue!;
        return JsonSerializer.Deserialize<T>(bytes)!;
    }


    private IDatabase GetDatabase<T>()
    {
        var dbIndex = ToDatabaseIndex(typeof(T));
        return _multiplexer.GetDatabase(dbIndex);
    }


    private static int ToDatabaseIndex<T>(T type)
        => type switch
        {
            WarpEntry => 1,
            ImageEntry => 2,
            _ => 0
        };


    private readonly IConnectionMultiplexer _multiplexer;
}