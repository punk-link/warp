using CSharpFunctionalExtensions;
using Microsoft.Extensions.Caching.Memory;
using Warp.WebApp.Data.Redis;
using Warp.WebApp.Extensions.Logging;

namespace Warp.WebApp.Data;

public sealed class DataStorage : IDataStorage
{
    public DataStorage(ILoggerFactory loggerFactory, IMemoryCache memoryCache, IDistributedStorage distributedStorage)
    {
        _logger = loggerFactory.CreateLogger<DataStorage>();
        _memoryCache = memoryCache;
        _distributedStorage = distributedStorage;
    }


    public Task<long> AddAndGetCounter(string key)
        => _distributedStorage.AddAndGetCounter(key);

    public async ValueTask<bool> Contains<T>(string key)
    {
        if (_memoryCache.TryGetValue(key, out _))
            return true;

        return await _distributedStorage.Contains<T>(key);
    }


    public void Remove<T>(string key)
    {
        _memoryCache.Remove(key);
        _distributedStorage.Remove<T>(key);
    }


    public async Task<Result> Set<T>(string key, T value, TimeSpan expiresIn)
    {
        if (value is null || IsDefaultStruct(value))
        {
            _logger.LogSetDefaultCacheValueError(value?.ToString());
            return Result.Failure("Can't store a default value.");
        }

        _memoryCache.Set(key, value, expiresIn);
        await _distributedStorage.Set(key, value, expiresIn);

        return Result.Success();
    }


    public async ValueTask<T?> TryGet<T>(string key)
    {
        if (_memoryCache.TryGetValue(key, out T? value))
            return value!;

        return await _distributedStorage.TryGet<T>(key);
    }


    private static bool IsDefaultStruct<T>(T value)
        => IsUserDefinedStruct(typeof(T)) && value!.Equals(default(T));


    private static bool IsUserDefinedStruct(Type type) 
        => type is { 
            IsValueType: true, 
            IsEnum: false, 
            IsPrimitive: false
        };


    private readonly ILogger<DataStorage> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedStorage _distributedStorage;
}