using Warp.WebApp.Constants.Logging;

namespace Warp.WebApp.Extensions.Logging;

internal static partial class LogMessages
{
    [LoggerMessage(LoggingConstants.ServerError, LogLevel.Error, "An error occurred during the request {RequestId}.")]
    public static partial void LogGenericServerError(this ILogger logger, string? requestId);

    [LoggerMessage(LoggingConstants.ServerErrorWithMessage, LogLevel.Error, "An error occurred during the request {RequestId} - '{ErrorMessage}'.")]
    public static partial void LogGenericServerError(this ILogger logger, string? requestId, string errorMessage);

    [LoggerMessage(LoggingConstants.ServiceUnavailable, LogLevel.Error, "503 Service Unavailable {RequestId} - '{ErrorMessage}'.")]
    public static partial void LogServiceUnavailable(this ILogger logger, string? requestId, string errorMessage);


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

    [LoggerMessage(LoggingConstants.PartialViewNotFound, LogLevel.Critical, "Partial view '{PartialViewName}' not found - '{ErrorMessage}'.")]
    public static partial void LogPartialViewNotFound(this ILogger logger, string partialViewName, string errorMessage);

    [LoggerMessage(LoggingConstants.PartialViewRenderingError, LogLevel.Critical, "Error rendering partial view '{PartialViewName}' - '{ErrorMessage}'.")]
    public static partial void LogPartialViewRenderingError(this ILogger logger, string partialViewName, string errorMessage);

    [LoggerMessage(LoggingConstants.ActionContextNotFound, LogLevel.Critical, "Action context not found while trying to build an image URL from a Razor page.")]
    public static partial void LogActionContextNotFound(this ILogger logger);

    [LoggerMessage(LoggingConstants.ImageControllerGetMethodNotFound, LogLevel.Critical, "Get image controller's method not found.")]
    public static partial void LogImageControllerGetMethodNotFound(this ILogger logger);
}