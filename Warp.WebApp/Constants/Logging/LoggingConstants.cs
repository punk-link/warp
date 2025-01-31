﻿namespace Warp.WebApp.Constants.Logging;

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
    public const int LocalConfigurationIsInUse = 11_201;
    public const int OptionsValidationException = 11_301;

    // Infrastructure
    public const int DefaultCacheValueError = 12_001;
    public const int PartialViewNotFound = 12_101;
    public const int PartialViewRenderingError = 12_102;
    public const int ActionContextNotFound = 12_103;
    public const int ImageControllerGetMethodNotFound = 12_104;
    public const int ImageUploadError = 12_201;
    public const int ImageDownloadError = 12_202;
    public const int ImageRemovalError = 12_203;
    public const int FileUploadException = 12_301;
    public const int UnverifiedFileSignatureError = 12_302;
    public const int FileSignatureVerificationError = 12_303;

    // Domain
    public const int WarpContentEmpty = 20_001;
    public const int WarpExpirationPeriodEmpty = 20_002;
    public const int ImageRemovalDomainError = 20_101;
}