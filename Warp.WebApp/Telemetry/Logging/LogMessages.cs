// This file is auto-generated. Do not edit directly.
// Generated on: 2025-04-22 12:14:35 UTC
using Microsoft.Extensions.Logging;
using System;
using Warp.WebApp.Constants.Logging;

namespace Warp.WebApp.Telemetry.Logging;

internal static partial class LogMessages
{
    // Generic
    [LoggerMessage((int)LogEvents.ServerError, LogLevel.Error, "An unexpected error occurred while processing the request. Request ID: '{RequestId}'.")]
    public static partial void LogServerError(this ILogger logger, string? requestId);

    [LoggerMessage((int)LogEvents.ServerErrorWithMessage, LogLevel.Error, "An error occurred while processing the request. Request ID: {RequestId}. Details: '{ErrorMessage:string}'.")]
    public static partial void LogServerErrorWithMessage(this ILogger logger, string? requestId, string errorMessage);

    [LoggerMessage((int)LogEvents.ServiceUnavailable, LogLevel.Error, "Service unavailable (503). Request ID: {RequestId}. Details: '{ErrorMessage:string}'.")]
    public static partial void LogServiceUnavailable(this ILogger logger, string? requestId, string errorMessage);


    // Startup
    [LoggerMessage((int)LogEvents.RedisHostIsNotSpecified, LogLevel.Critical, "Startup error: The Redis host configuration is missing.")]
    public static partial void LogRedisHostIsNotSpecified(this ILogger logger);

    [LoggerMessage((int)LogEvents.RedisPortIsNotSpecified, LogLevel.Critical, "Startup error: The Redis port configuration is missing.")]
    public static partial void LogRedisPortIsNotSpecified(this ILogger logger);

    [LoggerMessage((int)LogEvents.RedisConnectionException, LogLevel.Critical, "Startup error: Failed to connect to Redis. Details: '{ErrorMessage:string}'.")]
    public static partial void LogRedisConnectionException(this ILogger logger, string errorMessage);

    [LoggerMessage((int)LogEvents.VaultConnectionException, LogLevel.Critical, "Startup error: Failed to connect to the Vault. Details: '{ErrorMessage:string}'.")]
    public static partial void LogVaultConnectionException(this ILogger logger, string errorMessage);

    [LoggerMessage((int)LogEvents.VaultSecretCastException, LogLevel.Critical, "Startup error: Failed to cast retrieved secrets to the expected type. Details: '{ErrorMessage:string}'.")]
    public static partial void LogVaultSecretCastException(this ILogger logger, string errorMessage);

    [LoggerMessage((int)LogEvents.LocalConfigurationIsInUse, LogLevel.Information, "Local configuration is in use.")]
    public static partial void LogLocalConfigurationIsInUse(this ILogger logger);

    [LoggerMessage((int)LogEvents.OptionsValidationException, LogLevel.Error, "Startup error: Options validation exception occurred. Details: '{ErrorMessage:string}'.")]
    public static partial void LogOptionsValidationException(this ILogger logger, string errorMessage);


    // Infrastructure
    [LoggerMessage((int)LogEvents.DefaultCacheValueError, LogLevel.Warning, "Unable to store a default value {CacheValue}.")]
    public static partial void LogDefaultCacheValueError(this ILogger logger, string? cacheValue);

    [LoggerMessage((int)LogEvents.PartialViewNotFound, LogLevel.Critical, "Partial view '{PartialViewName:string}' could not be found. Details: '{ErrorMessage:string}'.")]
    public static partial void LogPartialViewNotFound(this ILogger logger, string partialViewName, string errorMessage);

    [LoggerMessage((int)LogEvents.PartialViewRenderingError, LogLevel.Critical, "An error occurred while rendering the partial view '{PartialViewName:string}'. Details: '{ErrorMessage:string}'.")]
    public static partial void LogPartialViewRenderingError(this ILogger logger, string partialViewName, string errorMessage);

    [LoggerMessage((int)LogEvents.ActionContextNotFound, LogLevel.Critical, "Action context not found while trying to build an image URL from a Razor page.")]
    public static partial void LogActionContextNotFound(this ILogger logger);

    [LoggerMessage((int)LogEvents.ImageControllerGetMethodNotFound, LogLevel.Critical, "Get image controller's method not found.")]
    public static partial void LogImageControllerGetMethodNotFound(this ILogger logger);

    [LoggerMessage((int)LogEvents.ImageUploadError, LogLevel.Error, "An error occurred while uploading the image. Details: '{ErrorMessage:string}'.")]
    public static partial void LogImageUploadError(this ILogger logger, string errorMessage);

    [LoggerMessage((int)LogEvents.ImageDownloadError, LogLevel.Error, "An error occurred while downloading the image. Details: '{ErrorMessage:string}'.")]
    public static partial void LogImageDownloadError(this ILogger logger, string errorMessage);

    [LoggerMessage((int)LogEvents.ImageRemovalError, LogLevel.Error, "An error occurred while removing the image. Details: '{ErrorMessage:string}'.")]
    public static partial void LogImageRemovalError(this ILogger logger, string errorMessage);

    [LoggerMessage((int)LogEvents.FileUploadException, LogLevel.Error, "An error occurred while uploading the file. Details: '{ErrorMessage:string}'.")]
    public static partial void LogFileUploadException(this ILogger logger, string errorMessage);

    [LoggerMessage((int)LogEvents.UnverifiedFileSignatureError, LogLevel.Error, "File signature verification failed. Details: '{ErrorMessage:string}'.")]
    public static partial void LogUnverifiedFileSignatureError(this ILogger logger, string errorMessage);

    [LoggerMessage((int)LogEvents.FileSignatureVerificationError, LogLevel.Warning, "File signature verification failed for files with extension '{FileExtension:string}', '{HeaderBytes:string?}'.")]
    public static partial void LogFileSignatureVerificationError(this ILogger logger, string fileExtension, string? headerBytes);


    // Domain
    [LoggerMessage((int)LogEvents.ImageRemovalDomainError, LogLevel.Error, "An error occurred in the domain layer while removing the image '{ImageId:Guid}'. Details: '{ErrorMessage:string}'.")]
    public static partial void LogImageRemovalDomainError(this ILogger logger, Guid imageId, string errorMessage);


    // Domain.Creator
    [LoggerMessage((int)LogEvents.CreatorIdIsNull, LogLevel.Warning, "The creator ID is not provided.")]
    public static partial void LogCreatorIdIsNull(this ILogger logger);


    // Domain.Entry
    [LoggerMessage((int)LogEvents.CantAttachEntryToCreator, LogLevel.Error, "Can't attach the entry '{EntryId:Guid}' to the creator '{CreatorId:Guid}' due to an internal server error.")]
    public static partial void LogCantAttachEntryToCreator(this ILogger logger, Guid entryId, Guid creatorId);
}
