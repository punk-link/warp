namespace Warp.WebApp.Data.Redis;

/// <summary>
/// Provides Redis database index mappings for common application concerns.
/// </summary>
public static class RedisDatabaseIndexes
{
    /// <summary>
    /// Redis database index used to store string-based cache entries and lifecycle scheduling members.
    /// </summary>
    public const int EntryLifecycleIndex = 4;
}
