using CSharpFunctionalExtensions;
using Microsoft.Extensions.Caching.Memory;
using Warp.WebApp.Data.Redis;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Telemetry.Logging;

namespace Warp.WebApp.Data;

public sealed class DataStorage : IDataStorage
{
    public DataStorage(ILoggerFactory loggerFactory, IMemoryCache memoryCache, IDistributedStorage distributedStorage)
    {
        _logger = loggerFactory.CreateLogger<DataStorage>();
        _memoryCache = memoryCache;
        _distributedStorage = distributedStorage;
    }


    public async Task<long> AddAndGetCounter(string key, CancellationToken cancellationToken)
        => await _distributedStorage.AddAndGetCounter(key, cancellationToken);


    public async Task<UnitResult<DomainError>> AddToSet<T>(string key, T value, TimeSpan expiresIn, CancellationToken cancellationToken)
    {
        if (value is null || IsDefaultStruct(value))
        {
            _logger.LogDefaultCacheValueError(value?.ToString());
            return DomainErrors.DefaultCacheValueError(value?.ToString());
        }

        await _distributedStorage.AddToSet(key, value, expiresIn, cancellationToken);

        if(!_memoryCache.TryGetValue(key, out HashSet<T>? set))
            set = [];

        set!.Add(value);
        _memoryCache.Set(key, set, expiresIn);

        return UnitResult.Success<DomainError>();
    }


    public async ValueTask<bool> Contains<T>(string key, CancellationToken cancellationToken)
    {
        if (_memoryCache.TryGetValue(key, out _))
            return true;

        return await _distributedStorage.Contains<T>(key, cancellationToken);
    }


    public async ValueTask<bool> ContainsInSet<T>(string key, T value, CancellationToken cancellationToken)
    {
        if (!_memoryCache.TryGetValue(key, out HashSet<T>? set))
            return await _distributedStorage.ContainsInSet(key, value, cancellationToken);
        
        if (set!.Contains(value))
            return true;

        return await _distributedStorage.ContainsInSet(key, value, cancellationToken);
    }


    public async Task Remove<T>(string key, CancellationToken cancellationToken)
    {
        _memoryCache.Remove(key);
        await _distributedStorage.Remove<T>(key, cancellationToken);
    }


    public async Task<UnitResult<DomainError>> Set<T>(string key, T value, TimeSpan expiresIn, CancellationToken cancellationToken)
    {
        if (value is null || IsDefaultStruct(value))
        {
            _logger.LogDefaultCacheValueError(value?.ToString());
            return DomainErrors.DefaultCacheValueError(value?.ToString());
        }

        _memoryCache.Set(key, value, expiresIn);

        await _distributedStorage.Set(key, value, expiresIn, cancellationToken);

        return UnitResult.Success<DomainError>();
    }


    public async ValueTask<T?> TryGet<T>(string key, CancellationToken cancellationToken)
    {
        if (_memoryCache.TryGetValue(key, out T? value))
            return value;

        return await _distributedStorage.TryGet<T>(key, cancellationToken);
    }


    public async ValueTask<HashSet<T>> TryGetSet<T>(string key, CancellationToken cancellationToken)
    {
        return await _distributedStorage.TryGetSet<T>(key, cancellationToken);
    }


    private static bool IsDefaultStruct<T>(T value)
        => IsUserDefinedStruct(typeof(T)) && value!.Equals(default(T));


    private static bool IsUserDefinedStruct(Type type)
        => type is
        {
            IsValueType: true,
            IsEnum: false,
            IsPrimitive: false
        };


    private readonly ILogger<DataStorage> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedStorage _distributedStorage;
}