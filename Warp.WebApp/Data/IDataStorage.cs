using CSharpFunctionalExtensions;

namespace Warp.WebApp.Data;

public interface IDataStorage
{
    void Remove<T>(string key);

    Task<Result> Set<T>(string key, T value, TimeSpan expiresIn);

    ValueTask<T?> TryGet<T>(string key);
}