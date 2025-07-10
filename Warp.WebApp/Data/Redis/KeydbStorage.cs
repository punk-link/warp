using StackExchange.Redis;
using System.Text.Json;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Entries;
using Warp.WebApp.Models.Images;
using Warp.WebApp.Services.Encryption;

namespace Warp.WebApp.Data.Redis;

public sealed class KeyDbStorage : IDistributedStorage
{
    public KeyDbStorage(IConnectionMultiplexer multiplexer, IEncryptionService encryptionService)
    {
        _multiplexer = multiplexer;
        _encryptionService = encryptionService;
    }


    public async Task<long> AddAndGetCounter(string key, CancellationToken cancellationToken)
    {
        var db = GetDatabase<string>();
        var task = db.StringIncrementAsync(key);

        return await ExecuteOrCancel(task, cancellationToken);
    }


    public async Task AddToSet<T>(string key, T value, TimeSpan expiresIn, CancellationToken cancellationToken)
    {
        var encryptedValue = await Encrypt(value);
        
        var db = GetDatabase<T>();
        var transaction = db.CreateTransaction();
        var transactionalTask = Task.WhenAll(transaction.SetAddAsync(key, encryptedValue), transaction.KeyExpireAsync(key, expiresIn));

        var isExecuted = await ExecuteOrCancel(transaction.ExecuteAsync(), cancellationToken);
        if (isExecuted)
            await transactionalTask;
    }


    public async Task<bool> Contains<T>(string key, CancellationToken cancellationToken)
    {
        var db = GetDatabase<T>();
        var task = db.KeyExistsAsync(key);

        return await ExecuteOrCancel(task, cancellationToken);
    }


    public async Task<bool> ContainsInSet<T>(string key, T value, CancellationToken cancellationToken)
    {
        var encryptedValue = await Encrypt(value);
        
        var db = GetDatabase<T>();
        var task = db.SetContainsAsync(key, encryptedValue);

        return await ExecuteOrCancel(task, cancellationToken);
    }


    public async Task Remove<T>(string key, CancellationToken cancellationToken)
    {
        var db = GetDatabase<T>();
        var task = db.KeyDeleteAsync(key);

        await ExecuteOrCancel(task, cancellationToken);
    }


    public async Task Set<T>(string key, T value, TimeSpan expiresIn, CancellationToken cancellationToken)
    {
        var encryptedValue = await Encrypt(value);
        
        var db = GetDatabase<T>();
        var task = db.StringSetAsync(key, encryptedValue, expiresIn);

        await ExecuteOrCancel(task, cancellationToken);
    }


    public async Task<T?> TryGet<T>(string key, CancellationToken cancellationToken)
    {
        var db = GetDatabase<T>();
        var task = db.StringGetAsync(key);

        var completedTask = await ExecuteOrCancel(task, cancellationToken);
        if (!completedTask.HasValue)
            return default;

        return await Decrypt<T>(completedTask!);
    }


    public async Task<HashSet<T>> TryGetSet<T>(string key, CancellationToken cancellationToken)
    {
        var db = GetDatabase<T>();
        var task = db.SetMembersAsync(key);

        var members = await ExecuteOrCancel(task, cancellationToken);
        if (members == null || members.Length == 0)
            return Enumerable.Empty<T>().ToHashSet();

        var results = new HashSet<T>();
        foreach (var member in members)
        {
            var value = await Decrypt<T>(member!);
            results.Add(value);
        }

        return results;
    }


    private async ValueTask<T> Decrypt<T>(byte[] encryptedBytes)
    {
        var decryptedBytes = await _encryptionService.Decrypt(encryptedBytes);
        return JsonSerializer.Deserialize<T>(decryptedBytes)!;
    }


    private async ValueTask<byte[]> Encrypt<T>(T value)
    {
        var serialized = JsonSerializer.SerializeToUtf8Bytes(value);
        return await _encryptionService.Encrypt(serialized);
    }


    private IDatabase GetDatabase<T>()
    {
        var dbIndex = ToDatabaseIndex(typeof(T));
        return _multiplexer.GetDatabase(dbIndex);
    }


    private static async Task<T?> ExecuteOrCancel<T>(Task<T?> task, CancellationToken cancellationToken)
    {
        var completedTask = await Task.WhenAny(task, Task.Delay(Timeout.Infinite, cancellationToken));
        if (completedTask == task)
            return await task;

        cancellationToken.ThrowIfCancellationRequested();
        return default;
    }


    private static int ToDatabaseIndex<T>(T type)
        => type switch
        {
            EntryInfo => 1,
            ImageInfo => 2,
            Report => 3,
            string => 4,
            Guid => 5,
            _ => 0
        };


    private readonly IConnectionMultiplexer _multiplexer;
    private readonly IEncryptionService _encryptionService;
}