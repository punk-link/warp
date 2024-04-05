namespace Warp.WebApp.Data.Redis;

public interface IDistributedStorage
{
    public Task<long> AddAndGetCounter(string key, CancellationToken cancellationToken);
    public Task<bool> Contains<T>(string key, CancellationToken cancellationToken);
    public void Remove<T>(string key);
    public Task Set<T>(string key, T value, TimeSpan expiresIn, CancellationToken cancellationToken);
    public Task<T?> TryGet<T>(string key, CancellationToken cancellationToken);
}