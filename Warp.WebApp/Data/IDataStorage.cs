using CSharpFunctionalExtensions;

namespace Warp.WebApp.Data;

public interface IDataStorage
{
    public Task<long> AddAndGetCounter(string key, CancellationToken cancellationToken);
    public ValueTask<bool> Contains<T>(string key, CancellationToken cancellationToken);
    public Task Remove<T>(string key, CancellationToken cancellationToken);
    public Task<Result> Set<T>(string key, T value, TimeSpan expiresIn, CancellationToken cancellationToken);
    public ValueTask<T?> TryGet<T>(string key, CancellationToken cancellationToken);
    public ValueTask<List<T>> TryGetList<T>(string key, CancellationToken cancellationToken);
    public Task<Result> CrossValueSet<K, V>(string keyK, K valueK, TimeSpan expiresInK, string keyV, V valueV, TimeSpan expiresInV, CancellationToken cancellationToken);
}