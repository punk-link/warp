// This file is auto-generated. Do not edit directly.
// Generated on: 2025-04-24 07:49:40 UTC
using Warp.WebApp.Constants.Logging;
using Warp.WebApp.Models.Errors;

namespace Warp.WebApp.Errors;

/// <summary>
/// Provides static methods to create domain error instances
/// </summary>
public static class DomainErrors
{
    // Generic
    public static DomainError ServerError()
        => new(LogEvents.ServerError);

    public static DomainError ServerErrorWithMessage(string? 0)
        => new(LogEvents.ServerErrorWithMessage, string.Format("An error occurred while processing the request. Details: '{0}'.", 0));

    public static DomainError ServiceUnavailable()
        => new(LogEvents.ServiceUnavailable);


    // Startup
    public static DomainError RedisHostIsNotSpecified()
        => new(LogEvents.RedisHostIsNotSpecified);

    public static DomainError RedisPortIsNotSpecified()
        => new(LogEvents.RedisPortIsNotSpecified);

    public static DomainError RedisConnectionException()
        => new(LogEvents.RedisConnectionException);

    public static DomainError VaultConnectionException()
        => new(LogEvents.VaultConnectionException);

    public static DomainError VaultSecretCastException()
        => new(LogEvents.VaultSecretCastException);

    public static DomainError LocalConfigurationIsInUse()
        => new(LogEvents.LocalConfigurationIsInUse);

    public static DomainError OptionsValidationException()
        => new(LogEvents.OptionsValidationException);


    // Infrastructure
    public static DomainError DefaultCacheValueError()
        => new(LogEvents.DefaultCacheValueError);

    public static DomainError PartialViewNotFound()
        => new(LogEvents.PartialViewNotFound);

    public static DomainError PartialViewRenderingError()
        => new(LogEvents.PartialViewRenderingError);

    public static DomainError ActionContextNotFound()
        => new(LogEvents.ActionContextNotFound);

    public static DomainError ImageControllerGetMethodNotFound()
        => new(LogEvents.ImageControllerGetMethodNotFound);

    [Obsolete("This error is obsolete. Do not use.")]
    public static DomainError ImageUploadError()
        => new(LogEvents.ImageUploadError);

    [Obsolete("This error is obsolete. Do not use.")]
    public static DomainError ImageDownloadError()
        => new(LogEvents.ImageDownloadError);

    [Obsolete("This error is obsolete. Do not use.")]
    public static DomainError ImageRemovalError()
        => new(LogEvents.ImageRemovalError);

    public static DomainError FileUploadException()
        => new(LogEvents.FileUploadException);

    public static DomainError UnverifiedFileSignatureError()
        => new(LogEvents.UnverifiedFileSignatureError);

    public static DomainError FileSignatureVerificationError()
        => new(LogEvents.FileSignatureVerificationError);


    // Domain
    public static DomainError WarpContentEmpty()
        => new(LogEvents.WarpContentEmpty);

    public static DomainError WarpExpirationPeriodEmpty()
        => new(LogEvents.WarpExpirationPeriodEmpty);

    public static DomainError ImageRemovalDomainError()
        => new(LogEvents.ImageRemovalDomainError);

    public static DomainError IdDecodingError()
        => new(LogEvents.IdDecodingError);

    public static DomainError UnauthorizedError()
        => new(LogEvents.UnauthorizedError);


    // Domain.Creator
    public static DomainError CreatorIdIsNull()
        => new(LogEvents.CreatorIdIsNull);

    public static DomainError CreatorIdIsNotFound()
        => new(LogEvents.CreatorIdIsNotFound);


    // Domain.Entry
    public static DomainError CantAttachEntryToCreator()
        => new(LogEvents.CantAttachEntryToCreator);

    public static DomainError EntryModelValidationError()
        => new(LogEvents.EntryModelValidationError);

    public static DomainError NoPermissionError()
        => new(LogEvents.NoPermissionError);

    public static DomainError EntryNotFound()
        => new(LogEvents.EntryNotFound);

    public static DomainError EntryEditModeMismatch()
        => new(LogEvents.EntryEditModeMismatch);

    public static DomainError EntryCannotBeEditedAfterViewed()
        => new(LogEvents.EntryCannotBeEditedAfterViewed);

    public static DomainError EntryInfoModelValidationError()
        => new(LogEvents.EntryInfoModelValidationError);


    // Domain.Image
    public static DomainError UnsupportedFileExtension(string? 0, string? 1)
        => new(LogEvents.UnsupportedFileExtension, string.Format("The file extension '{0}' is not supported. Please use one of the following extensions: {1}.", 0, 1));

    public static DomainError ImageAlreadyExists(string? 0)
        => new(LogEvents.ImageAlreadyExists, string.Format("The image '{0}' you are trying to upload already exists.", 0));


    // Domain.File
    public static DomainError S3ListObjectsError()
        => new(LogEvents.S3ListObjectsError);

    public static DomainError S3DeleteObjectError()
        => new(LogEvents.S3DeleteObjectError);

    public static DomainError S3UploadObjectError()
        => new(LogEvents.S3UploadObjectError);

    public static DomainError S3GetObjectError()
        => new(LogEvents.S3GetObjectError);

    public static DomainError FileNameIsMissing()
        => new(LogEvents.FileNameIsMissing);

    public static DomainError FileIsEmpty()
        => new(LogEvents.FileIsEmpty);

    public static DomainError FileSizeExceeded()
        => new(LogEvents.FileSizeExceeded);

    public static DomainError FileTypeNotPermitted()
        => new(LogEvents.FileTypeNotPermitted);

    public static DomainError MultipartReaderError()
        => new(LogEvents.MultipartReaderError);

    public static DomainError MultipartContentTypeBoundaryError()
        => new(LogEvents.MultipartContentTypeBoundaryError);

    public static DomainError MultipartBoundaryLengthLimitExceeded()
        => new(LogEvents.MultipartBoundaryLengthLimitExceeded);
}
