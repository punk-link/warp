using Warp.WebApp.Constants.Logging;

namespace Warp.WebApp.Extensions.Logging;

internal static partial class LogMessages
{
    [LoggerMessage(LoggingConstants.ServerError, LogLevel.Warning, "An error occurred during the request {RequestId}.")]
    public static partial void LogGenericServerError(this ILogger logger, string? requestId);
    [LoggerMessage(LoggingConstants.ServerError, LogLevel.Warning, "An error occurred during the request {RequestId}: {ErrorMessage}.")]
    public static partial void LogGenericServerError(this ILogger logger, string? requestId, string errorMessage);
}