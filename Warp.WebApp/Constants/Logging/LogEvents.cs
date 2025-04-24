// This file is auto-generated. Do not edit directly.
// Generated on: 2025-04-24 07:49:40 UTC
using System;
using System.ComponentModel;
using System.Linq;
using Warp.WebApp.Attributes;

namespace Warp.WebApp.Constants.Logging;

public enum LogEvents
{
    // Generic
    [Description("An unexpected error occurred while processing the request. Request ID: '{RequestId}'.")]
    [HttpStatusCode(500)]
    ServerError = 10001,
    [Description("An error occurred while processing the request. Details: '{0}'.")]
    [HttpStatusCode(500)]
    ServerErrorWithMessage = 10002,
    [Description("Service unavailable (503). Request ID: {RequestId}. Details: '{ErrorMessage:string}'.")]
    [HttpStatusCode(503)]
    ServiceUnavailable = 10003,


    // Startup
    [Description("Startup error: The Redis host configuration is missing.")]
    [HttpStatusCode(500)]
    RedisHostIsNotSpecified = 11001,
    [Description("Startup error: The Redis port configuration is missing.")]
    [HttpStatusCode(500)]
    RedisPortIsNotSpecified = 11002,
    [Description("Startup error: Failed to connect to Redis. Details: '{ErrorMessage:string}'.")]
    [HttpStatusCode(500)]
    RedisConnectionException = 11003,
    [Description("Startup error: Failed to connect to the Vault. Details: '{ErrorMessage:string}'.")]
    [HttpStatusCode(500)]
    VaultConnectionException = 11101,
    [Description("Startup error: Failed to cast retrieved secrets to the expected type. Details: '{ErrorMessage:string}'.")]
    [HttpStatusCode(500)]
    VaultSecretCastException = 11102,
    [Description("Local configuration is in use.")]
    [HttpStatusCode(500)]
    LocalConfigurationIsInUse = 11201,
    [Description("Startup error: Options validation exception occurred. Details: '{ErrorMessage:string}'.")]
    [HttpStatusCode(500)]
    OptionsValidationException = 11301,


    // Infrastructure
    [Description("Unable to store a default value {CacheValue}.")]
    [HttpStatusCode(500)]
    DefaultCacheValueError = 12001,
    [Description("Partial view '{PartialViewName:string}' could not be found. Details: '{ErrorMessage:string}'.")]
    [HttpStatusCode(500)]
    PartialViewNotFound = 12101,
    [Description("An error occurred while rendering the partial view '{PartialViewName:string}'. Details: '{ErrorMessage:string}'.")]
    [HttpStatusCode(500)]
    PartialViewRenderingError = 12102,
    [Description("Action context not found while trying to build an image URL from a Razor page.")]
    [HttpStatusCode(500)]
    ActionContextNotFound = 12103,
    [Description("Get image controller's method not found.")]
    [HttpStatusCode(500)]
    ImageControllerGetMethodNotFound = 12104,
    [Obsolete("This logging event is obsolete and will be removed in a future version.")]
    [Description("An error occurred while uploading the image. Details: '{ErrorMessage:string}'.")]
    [HttpStatusCode(500)]
    ImageUploadError = 12201,
    [Obsolete("This logging event is obsolete and will be removed in a future version.")]
    [Description("An error occurred while downloading the image. Details: '{ErrorMessage:string}'.")]
    [HttpStatusCode(500)]
    ImageDownloadError = 12202,
    [Obsolete("This logging event is obsolete and will be removed in a future version.")]
    [Description("An error occurred while removing the image. Details: '{ErrorMessage:string}'.")]
    [HttpStatusCode(500)]
    ImageRemovalError = 12203,
    [Description("An error occurred while uploading the file. Details: '{ErrorMessage:string}'.")]
    [HttpStatusCode(400)]
    FileUploadException = 12301,
    [Description("File signature verification failed. Details: '{ErrorMessage:string}'.")]
    [HttpStatusCode(415)]
    UnverifiedFileSignatureError = 12302,
    [Description("File signature verification failed for files with extension '{FileExtension:string}', '{HeaderBytes:string?}'.")]
    [HttpStatusCode(415)]
    FileSignatureVerificationError = 12303,


    // Domain
    [Description("The entry content is empty.")]
    [HttpStatusCode(400)]
    WarpContentEmpty = 20001,
    [Description("The entry expiration period is not specified.")]
    [HttpStatusCode(400)]
    WarpExpirationPeriodEmpty = 20002,
    [Description("An error occurred in the domain layer while removing the image '{ImageId:Guid}'. Details: '{ErrorMessage:string}'.")]
    [HttpStatusCode(400)]
    ImageRemovalDomainError = 20101,
    [Description("An error occurred while decoding the ID.")]
    [HttpStatusCode(400)]
    IdDecodingError = 20102,
    [Description("You do not have permission to perform this action.")]
    [HttpStatusCode(403)]
    UnauthorizedError = 20103,


    // Domain.Creator
    [Description("The creator ID is not provided.")]
    [HttpStatusCode(400)]
    CreatorIdIsNull = 20201,
    [Description("The creator ID is not found.")]
    [HttpStatusCode(400)]
    CreatorIdIsNotFound = 20202,


    // Domain.Entry
    [Description("An error occurred while attaching the entry to the creator.")]
    [HttpStatusCode(500)]
    CantAttachEntryToCreator = 20301,
    [Description("The entry model is invalid. Please check the provided data.")]
    [HttpStatusCode(400)]
    EntryModelValidationError = 20302,
    [Description("You do not have permission to access this entry.")]
    [HttpStatusCode(403)]
    NoPermissionError = 20303,
    [Description("The entry was not found.")]
    [HttpStatusCode(404)]
    EntryNotFound = 20304,
    [Description("The entry edit mode is already set to and cannot be changed.")]
    [HttpStatusCode(400)]
    EntryEditModeMismatch = 20305,
    [Description("The entry cannot be edited after it has been viewed. Try to create a new entry.")]
    [HttpStatusCode(400)]
    EntryCannotBeEditedAfterViewed = 20306,
    [Description("The entry model is invalid. Please check the provided data.")]
    [HttpStatusCode(400)]
    EntryInfoModelValidationError = 20307,


    // Domain.Image
    [Description("The file extension '{0}' is not supported. Please use one of the following extensions: {1}.")]
    [HttpStatusCode(415)]
    UnsupportedFileExtension = 20401,
    [Description("The image '{0}' you are trying to upload already exists.")]
    [HttpStatusCode(400)]
    ImageAlreadyExists = 20402,


    // Domain.File
    [Description("An error occurred while listing objects.")]
    [HttpStatusCode(500)]
    S3ListObjectsError = 20501,
    [Description("An error occurred while deleting an object.")]
    [HttpStatusCode(500)]
    S3DeleteObjectError = 20502,
    [Description("An error occurred while uploading an object.")]
    [HttpStatusCode(500)]
    S3UploadObjectError = 20503,
    [Description("An error occurred while getting an object.")]
    [HttpStatusCode(500)]
    S3GetObjectError = 20504,
    [Description("The file name is missing. Please provide a valid file name.")]
    [HttpStatusCode(400)]
    FileNameIsMissing = 20505,
    [Description("The file is empty. Please provide a valid file.")]
    [HttpStatusCode(400)]
    FileIsEmpty = 20506,
    [Description("The file size is {Actual:long} bytes. Yoy can upload files up to {MaxFileSize:long} bytes.")]
    [HttpStatusCode(400)]
    FileSizeExceeded = 20507,
    [Description("The provided file type is not permitted.")]
    [HttpStatusCode(400)]
    FileTypeNotPermitted = 20508,
    [Description("An error occurred while creating the MultipartReader. Probably the request is not a multipart/form-data request.")]
    [HttpStatusCode(400)]
    MultipartReaderError = 20509,
    [Description("The content-type boundary is missing. Please provide a valid content-type boundary.")]
    [HttpStatusCode(400)]
    MultipartContentTypeBoundaryError = 20510,
    [Description("The multipart boundary length limit has been exceeded. Please provide a valid content-type boundary.")]
    [HttpStatusCode(400)]
    MultipartBoundaryLengthLimitExceeded = 20511,
}
