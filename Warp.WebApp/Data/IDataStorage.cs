using CSharpFunctionalExtensions;

namespace Warp.WebApp.Data;

public interface IDataStorage
{
    public Task<long> AddAndGetCounter(string key);
    public void Remove<T>(string key);
    public Task<Result> Set<T>(string key, T value, TimeSpan expiresIn);
    public ValueTask<T?> TryGet<T>(string key);
}