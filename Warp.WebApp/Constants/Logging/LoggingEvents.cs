// This file is auto-generated. Do not edit directly.
// Generated on: 2025-04-21 08:08:36 UTC
using System;
using System.ComponentModel;

namespace Warp.WebApp.Constants.Logging;

public enum LoggingEvents
{
    // Generic
    [Description("An unexpected error occurred while processing the request. Request ID: '{RequestId}'.")]
    ServerError = 10001,
    [Description("An error occurred while processing the request. Request ID: {RequestId}. Details: '{ErrorMessage:string}'.")]
    ServerErrorWithMessage = 10002,
    [Description("Service unavailable (503). Request ID: {RequestId}. Details: '{ErrorMessage:string}'.")]
    ServiceUnavailable = 10003,


    // Startup
    [Description("Startup error: The Redis host configuration is missing.")]
    RedisHostIsNotSpecified = 11001,
    [Description("Startup error: The Redis port configuration is missing.")]
    RedisPortIsNotSpecified = 11002,
    [Description("Startup error: Failed to connect to Redis. Details: '{ErrorMessage:string}'.")]
    RedisConnectionException = 11003,
    [Description("Startup error: Failed to connect to the Vault. Details: '{ErrorMessage:string}'.")]
    VaultConnectionException = 11101,
    [Description("Startup error: Failed to cast retrieved secrets to the expected type. Details: '{ErrorMessage:string}'.")]
    VaultSecretCastException = 11102,
    [Description("Local configuration is in use.")]
    LocalConfigurationIsInUse = 11201,
    [Description("Startup error: Options validation exception occurred. Details: '{ErrorMessage:string}'.")]
    OptionsValidationException = 11301,


    // Infrastructure
    [Description("Unable to store a default value {CacheValue}.")]
    DefaultCacheValueError = 12001,
    [Description("Partial view '{PartialViewName:string}' could not be found. Details: '{ErrorMessage:string}'.")]
    PartialViewNotFound = 12101,
    [Description("An error occurred while rendering the partial view '{PartialViewName:string}'. Details: '{ErrorMessage:string}'.")]
    PartialViewRenderingError = 12102,
    [Description("Action context not found while trying to build an image URL from a Razor page.")]
    ActionContextNotFound = 12103,
    [Description("Get image controller's method not found.")]
    ImageControllerGetMethodNotFound = 12104,
    [Description("An error occurred while uploading the image. Details: '{ErrorMessage:string}'.")]
    ImageUploadError = 12201,
    [Description("An error occurred while downloading the image. Details: '{ErrorMessage:string}'.")]
    ImageDownloadError = 12202,
    [Description("An error occurred while removing the image. Details: '{ErrorMessage:string}'.")]
    ImageRemovalError = 12203,
    [Description("An error occurred while uploading the file. Details: '{ErrorMessage:string}'.")]
    FileUploadException = 12301,
    [Description("File signature verification failed. Details: '{ErrorMessage:string}'.")]
    UnverifiedFileSignatureError = 12302,
    [Description("File signature verification failed for files with extension '{FileExtension:string}', '{HeaderBytes:string?}'.")]
    FileSignatureVerificationError = 12303,


    // Domain
    [Description("The entry content is empty.")]
    WarpContentEmpty = 20001,
    [Description("The entry expiration period is not specified.")]
    WarpExpirationPeriodEmpty = 20002,
    [Description("An error occurred in the domain layer while removing the image '{ImageId:Guid}'. Details: '{ErrorMessage:string}'.")]
    ImageRemovalDomainError = 20101,
}
