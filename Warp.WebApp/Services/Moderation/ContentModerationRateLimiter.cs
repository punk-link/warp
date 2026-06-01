using Microsoft.Extensions.Options;
using Warp.WebApp.Models.Options;

namespace Warp.WebApp.Services.Moderation;

/// <summary>
/// Adaptive rate limiter for content moderation API calls.
/// Dynamically adjusts concurrency based on the rolling success rate and server-provided backoff signals.
/// </summary>
public sealed class ContentModerationRateLimiter : IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentModerationRateLimiter"/> class.
    /// </summary>
    /// <param name="options">Content moderation configuration.</param>
    public ContentModerationRateLimiter(IOptions<ContentModerationOptions> options)
    {
        _maxConcurrency = options.Value.MaxConcurrency;
        _successThreshold = options.Value.SuccessThreshold;
        _windowSize = options.Value.SuccessRateWindow;
        _currentConcurrency = options.Value.InitialConcurrency;
        _rateLimiterOptions = options.Value.RateLimiter;

        _semaphore = new SemaphoreSlim(_currentConcurrency, _maxConcurrency);
        _results = new Queue<(DateTimeOffset timestamp, bool success)>();
    }


    /// <summary>
    /// Gets the current concurrency level.
    /// </summary>
    public int CurrentConcurrency 
        => _currentConcurrency;


    /// <summary>
    /// Acquires a permit to execute a moderation API call.
    /// Honors any active server-provided backoff window.
    /// The returned token must be disposed after the call completes.
    /// </summary>
    /// <param name="cancellationToken">Token used to observe cancellation requests.</param>
    /// <returns>A disposable token that releases the permit when disposed.</returns>
    public async Task<IDisposable> Acquire(CancellationToken cancellationToken)
    {
        while (true)
        {
            var holdUntilTicks = Interlocked.Read(ref _holdUntilTicks);
            if (holdUntilTicks > 0)
            {
                var remaining = new DateTimeOffset(holdUntilTicks, TimeSpan.Zero) - DateTimeOffset.UtcNow + _minBackoff;
                if (remaining > TimeSpan.Zero)
                {
                    await Task.Delay(remaining, cancellationToken);
                    continue;
                }
            }

            await _semaphore.WaitAsync(cancellationToken);
            Interlocked.Increment(ref _inUse);

            // Re-check after acquiring the semaphore — a Retry-After may have arrived.
            var recheckTicks = Interlocked.Read(ref _holdUntilTicks);
            if (recheckTicks > 0)
            {
                var remaining = new DateTimeOffset(recheckTicks, TimeSpan.Zero) - DateTimeOffset.UtcNow + _minBackoff;
                if (remaining > TimeSpan.Zero)
                {
                    Interlocked.Decrement(ref _inUse);
                    _semaphore.Release();
                    await Task.Delay(remaining, cancellationToken);

                    continue;
                }
            }

            return new ReleaseToken(this);
        }
    }


    /// <summary>
    /// Records a successful API call and adjusts concurrency if the rolling window warrants it.
    /// </summary>
    public void RecordSuccess()
    {
        lock (_lock)
        {
            _results.Enqueue((DateTimeOffset.UtcNow, true));
            _consecutiveFailures = 0;
            AdjustConcurrency();
        }
    }


    /// <summary>
    /// Records a failed API call and adjusts concurrency if needed.
    /// </summary>
    /// <param name="isRateLimitError"><c>true</c> when the failure was an HTTP 429 rate-limit response.</param>
    public void RecordFailure(bool isRateLimitError)
    {
        lock (_lock)
        {
            _results.Enqueue((DateTimeOffset.UtcNow, false));
            _consecutiveFailures++;

            if (isRateLimitError || _consecutiveFailures >= _rateLimiterOptions.CircuitBreakerFailureThreshold)
                DecreaseConcurrency();
            else
                AdjustConcurrency();
        }
    }


    /// <summary>
    /// Records a server-provided <c>Retry-After</c> backoff to temporarily pause new acquisitions,
    /// and proactively decreases concurrency.
    /// </summary>
    /// <param name="delay">The retry delay indicated by the server.</param>
    public void RecordRetryAfter(in TimeSpan delay)
    {
        if (delay <= TimeSpan.Zero)
            return;

        // Atomically set the hold-until to the furthest-future time seen.
        var untilTicks = (DateTimeOffset.UtcNow + delay).Ticks;
        long observed;
        do
        {
            observed = Interlocked.Read(ref _holdUntilTicks);
            if (untilTicks <= observed)
                break;
        }
        while (Interlocked.CompareExchange(ref _holdUntilTicks, untilTicks, observed) != observed);

        lock (_lock)
            DecreaseConcurrency();
    }


    /// <summary>
    /// Releases the semaphore and other managed resources.
    /// </summary>
    public void Dispose() => _semaphore.Dispose();


    private void AdjustConcurrency()
    {
        CleanupOldResults();

        if (_results.Count < _rateLimiterOptions.MinimumSampleSize)
            return;

        var successCount = _results.Count(r => r.success);
        var successRate = (double)successCount / _results.Count;

        if (successRate >= _successThreshold && _currentConcurrency < _maxConcurrency)
            IncreaseConcurrency();
        else if (successRate < _successThreshold * _rateLimiterOptions.LowSuccessRateMultiplier)
            DecreaseConcurrency();
    }


    private void IncreaseConcurrency()
    {
        var increment = Math.Min(_rateLimiterOptions.ConcurrencyIncrement, _maxConcurrency - _currentConcurrency);
        if (increment <= 0)
            return;

        _currentConcurrency += increment;

        var desiredAvailable = Math.Max(0, _currentConcurrency - Volatile.Read(ref _inUse));
        var toRelease = Math.Max(0, desiredAvailable - _semaphore.CurrentCount);
        if (toRelease > 0)
            _semaphore.Release(toRelease);
    }


    private void DecreaseConcurrency()
    {
        var decrement = Math.Max(
            _rateLimiterOptions.MinimumConcurrencyDecrement,
            (int)Math.Ceiling(_currentConcurrency * _rateLimiterOptions.ConcurrencyDecreaseRatio));

        _currentConcurrency = Math.Max(_rateLimiterOptions.MinimumConcurrency, _currentConcurrency - decrement);

        var desiredAvailable = Math.Max(0, _currentConcurrency - Volatile.Read(ref _inUse));
        var excess = _semaphore.CurrentCount - desiredAvailable;
        while (excess > 0 && _semaphore.Wait(0))
            excess--;
    }


    private void CleanupOldResults()
    {
        var cutoff = DateTimeOffset.UtcNow - _windowSize;
        while (_results.Count > 0 && _results.Peek().timestamp < cutoff)
            _results.Dequeue();
    }


    private sealed class ReleaseToken : IDisposable
    {
        public ReleaseToken(ContentModerationRateLimiter limiter) 
            => _limiter = limiter;


        public void Dispose()
        {
            Interlocked.Decrement(ref _limiter._inUse);
            _limiter._semaphore.Release();
        }


        private readonly ContentModerationRateLimiter _limiter;
    }


    private int _consecutiveFailures;
    private int _currentConcurrency;
    private int _inUse;
    private long _holdUntilTicks;
    private readonly Lock _lock = new();
    private readonly int _maxConcurrency;
    private static readonly TimeSpan _minBackoff = TimeSpan.FromSeconds(1);
    private readonly Queue<(DateTimeOffset timestamp, bool success)> _results;
    private readonly ContentModerationRateLimiterOptions _rateLimiterOptions;
    private readonly SemaphoreSlim _semaphore;
    private readonly double _successThreshold;
    private readonly TimeSpan _windowSize;
}
