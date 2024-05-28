namespace Warp.WebApp.Data.Redis;

public interface IDistributedStorage
{
    public Task<long> AddAndGetCounter(string key, CancellationToken cancellationToken);
    public Task<bool> Contains<T>(string key, CancellationToken cancellationToken);
    public Task Remove<T>(string key, CancellationToken cancellationToken);
    public Task Set<T>(string key, T value, TimeSpan expiresIn, CancellationToken cancellationToken);
    public Task SetToList<T>(string key, T value, TimeSpan expiresIn, CancellationToken cancellationToken);
    public Task<T?> TryGet<T>(string key, CancellationToken cancellationToken);
}