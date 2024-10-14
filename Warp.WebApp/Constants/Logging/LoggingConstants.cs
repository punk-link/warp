namespace Warp.WebApp.Constants.Logging;

public static class LoggingConstants
{
    // Generic
    public const int ServerError = 10_001;
    public const int ServerErrorWithMessage = 10_002;
    public const int ServiceUnavailable = 10_003;

    // Startup
    public const int RedisHostIsNotSnspecified = 11_001;
    public const int RedisPortIsNotSnspecified = 11_002;
    public const int RedisConnectionException = 11_003;
    public const int VaultConnectionException = 11_101;
    public const int VaultSecretCastException = 11_102;

    // Infrastructure
    public const int DefaultCacheValueError = 12_001;
    public const int PartialViewNotFound = 12_101;
    public const int PartialViewRenderingError = 12_102;
    public const int ActionContextNotFound = 12_103;
    public const int ImageControllerGetMethodNotFound = 12_104;

    // Domain
    public const int WarpContentEmpty = 20_001;
    public const int WarpExpirationPeriodEmpty = 20_002;
}