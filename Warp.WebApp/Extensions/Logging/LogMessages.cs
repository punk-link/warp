using Warp.WebApp.Constants.Logging;

namespace Warp.WebApp.Extensions.Logging;

internal static partial class LogMessages
{
    [LoggerMessage(LoggingConstants.ServerError, LogLevel.Warning, "An error occurred during the request {RequestId}.")]
    public static partial void LogGenericServerError(this ILogger logger, string? requestId);

    [LoggerMessage(LoggingConstants.ServerError, LogLevel.Warning, "An error occurred during the request {RequestId}: {ErrorMessage}.")]
    public static partial void LogGenericServerError(this ILogger logger, string? requestId, string errorMessage);


    [LoggerMessage(LoggingConstants.RedisHostUnspecified, LogLevel.Critical, "Startup error: Redis host unspecified.")]
    public static partial void LogRedisHostUnspecified(this ILogger logger);

    [LoggerMessage(LoggingConstants.RedisPortUnspecified, LogLevel.Critical, "Startup error: Redis port unspecified.")]
    public static partial void LogRedisPortUnspecified(this ILogger logger);

    [LoggerMessage(LoggingConstants.RedisConnectionException, LogLevel.Critical, "Startup error: Redis connection exception occurred - '{ErrorMessage}'.")]
    public static partial void LogRedisConnectionException(this ILogger logger, string errorMessage);

    [LoggerMessage(LoggingConstants.VaultConnectionException, LogLevel.Critical, "Startup error: Vault connection exception occurred - '{ErrorMessage}'.")]
    public static partial void LogVaultConnectionException(this ILogger logger, string errorMessage);

    [LoggerMessage(LoggingConstants.VaultSecretCastException, LogLevel.Critical, "Startup error: can't cast obtained secrets to a typed object - '{ErrorMessage}'.")]
    public static partial void LogVaultSecretCastException(this ILogger logger, string errorMessage);


    [LoggerMessage(LoggingConstants.DefaultCacheValueError, LogLevel.Warning, "Can't store a default value {CacheValue}.")]
    public static partial void LogSetDefaultCacheValueError(this ILogger logger, string? cacheValue);
}