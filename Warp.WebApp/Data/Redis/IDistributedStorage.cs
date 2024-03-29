﻿namespace Warp.WebApp.Data.Redis;

public interface IDistributedStorage
{
    public Task<long> AddAndGetCounter(string key);
    public Task<bool> Contains<T>(string key);
    public void Remove<T>(string key);
    public Task Set<T>(string key, T value, TimeSpan expiresIn);
    public Task<T?> TryGet<T>(string key);
}