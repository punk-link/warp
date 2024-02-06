namespace Warp.WebApp.Data.Redis;

public interface IDistributedStorage
{
    void Remove<T>(string key);

    Task Set<T>(string key, T value, TimeSpan expiresIn);

    Task<T?> TryGet<T>(string key);
}