namespace Warp.WebApp.Constants.Logging;

public static class LoggingConstants
{
    // Generic
    public const int ServerError = 10_001;

    // Startup
    public const int RedisHostUnspecified = 11_001;
    public const int RedisPortUnspecified = 11_002;
    public const int RedisConnectionException = 11_003;
    public const int VaultConnectionException = 11_101;
    public const int VaultSecretCastException = 11_102;

    // Infrastructure
    public const int DefaultCacheValueError = 12_001;
    
    // Domain
    public const int WarpContentEmpty = 20_001;
    public const int WarpExpirationPeriodEmpty = 20_002;
}