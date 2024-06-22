using CSharpFunctionalExtensions;

namespace Warp.WebApp.Data;

public interface IDataStorage
{
    public Task<long> AddAndGetCounter(string key, CancellationToken cancellationToken);
    public Task<Result> AddToSet<T>(string key, T value, TimeSpan expiresIn, CancellationToken cancellationToken);
    public ValueTask<bool> Contains<T>(string key, CancellationToken cancellationToken);
    public Task Remove<T>(string key, CancellationToken cancellationToken);
    public Task<Result> Set<T>(string key, T value, TimeSpan expiresIn, CancellationToken cancellationToken);
    public ValueTask<T?> TryGet<T>(string key, CancellationToken cancellationToken);
    public ValueTask<HashSet<T>> TryGetSet<T>(string key, CancellationToken cancellationToken);
    public Task<bool> IsValueContainsInSet<T>(string key, T value, CancellationToken cancellationToken);
}