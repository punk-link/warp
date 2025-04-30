using CSharpFunctionalExtensions;
using Warp.WebApp.Models.Errors;

namespace Warp.WebApp.Data;

public interface IDataStorage
{
    public Task<long> AddAndGetCounter(string key, CancellationToken cancellationToken);
    public Task<UnitResult<DomainError>> AddToSet<T>(string key, T value, TimeSpan expiresIn, CancellationToken cancellationToken);
    public ValueTask<bool> Contains<T>(string key, CancellationToken cancellationToken);
    public ValueTask<bool> ContainsInSet<T>(string key, T value, CancellationToken cancellationToken);
    public Task Remove<T>(string key, CancellationToken cancellationToken);
    public Task<UnitResult<DomainError>> Set<T>(string key, T value, TimeSpan expiresIn, CancellationToken cancellationToken);
    public ValueTask<T?> TryGet<T>(string key, CancellationToken cancellationToken);
    public ValueTask<HashSet<T>> TryGetSet<T>(string key, CancellationToken cancellationToken);
}