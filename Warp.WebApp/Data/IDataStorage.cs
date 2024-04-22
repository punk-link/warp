using CSharpFunctionalExtensions;

namespace Warp.WebApp.Data;

public interface IDataStorage
{
    public Task<long> AddAndGetCounter(string key, CancellationToken cancellationToken);
    public ValueTask<bool> Contains<T>(string key, CancellationToken cancellationToken);
    public void Remove<T>(string key);
    public Task<Result> Set<T>(string key, T value, TimeSpan expiresIn, CancellationToken cancellationToken);
    public ValueTask<T?> TryGet<T>(string key, CancellationToken cancellationToken);
}