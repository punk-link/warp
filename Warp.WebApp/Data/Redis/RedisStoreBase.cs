using StackExchange.Redis;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Entries;
using Warp.WebApp.Models.Files;
using Warp.WebApp.Models.Images;
using Warp.WebApp.Models.Moderation;

namespace Warp.WebApp.Data.Redis;

/// <summary>
/// Provides shared Redis execution helpers for distributed storage implementations.
/// </summary>
public abstract class RedisStoreBase
{
    public RedisStoreBase(IConnectionMultiplexer multiplexer)
    {
        _multiplexer = multiplexer;
    }


    /// <summary>
    /// Awaits the provided task while honoring cancellation.
    /// </summary>
    /// <typeparam name="T">Type of the awaited task result.</typeparam>
    /// <param name="task">The task to execute.</param>
    /// <param name="cancellationToken">Token that signals cancellation.</param>
    /// <returns>The task result when completed successfully.</returns>
    protected static async Task<T> ExecuteOrCancel<T>(Task<T> task, CancellationToken cancellationToken)
    {
        var completedTask = await Task.WhenAny(task, Task.Delay(Timeout.Infinite, cancellationToken));
        if (completedTask == task)
            return await task;

        cancellationToken.ThrowIfCancellationRequested();
        return default!;
    }


    /// <summary>
    /// Awaits the provided task while honoring cancellation.
    /// </summary>
    /// <param name="task">The task to execute.</param>
    /// <param name="cancellationToken">Token that signals cancellation.</param>
    protected static async Task ExecuteOrCancel(Task task, CancellationToken cancellationToken)
    {
        var completedTask = await Task.WhenAny(task, Task.Delay(Timeout.Infinite, cancellationToken));
        if (completedTask == task)
        {
            await task;
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();
    }


    protected IDatabase GetDatabase<T>()
    {
        var dbIndex = ToDatabaseIndex(typeof(T));
        return _multiplexer.GetDatabase(dbIndex);
    }


    private static int ToDatabaseIndex(Type type)
    {
        var effectiveType = Nullable.GetUnderlyingType(type) ?? type;

        return effectiveType switch
        {
            _ when effectiveType == typeof(EntryInfo) => 1,
            _ when effectiveType == typeof(ImageInfo) => 2,
            _ when effectiveType == typeof(Report) => 3,
            _ when effectiveType == typeof(string) => 4,
            _ when effectiveType == typeof(Guid) => 5,
            _ when effectiveType == typeof(EntryImageLifecycle) => 6,
            _ when effectiveType == typeof(CachedImage) => 7,
            _ when effectiveType == typeof(EntryModerationJob) => 8,
            _ => 0
        };
    }


    protected readonly IConnectionMultiplexer _multiplexer;
}
