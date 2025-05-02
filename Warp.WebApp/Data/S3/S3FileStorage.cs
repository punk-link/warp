using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using Warp.WebApp.Attributes;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Models.Files;
using Warp.WebApp.Telemetry.Logging;

namespace Warp.WebApp.Data.S3;

/// <summary>
/// Implementation of <see cref="IS3FileStorage"/> that manages files in Amazon S3 storage.
/// </summary>
public class S3FileStorage : IS3FileStorage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="S3FileStorage"/> class.
    /// </summary>
    /// <param name="amazonS3Factory">Factory for creating Amazon S3 clients.</param>
    /// <param name="loggerFactory">Factory for creating loggers.</param>
    public S3FileStorage(IAmazonS3Factory amazonS3Factory, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<S3FileStorage>();

        _amazonS3Client = amazonS3Factory.CreateClient();
        _amazonS3Factory = amazonS3Factory;
    }


    /// <inheritdoc/>
    [TraceMethod]
    public async Task<Result<HashSet<string>, DomainError>> Contains(string prefix, List<string> keys, CancellationToken cancellationToken)
    {
        string error;
        try
        {
            prefix = BuildPrefix(prefix);
            var request = new ListObjectsV2Request
            {
                BucketName = _amazonS3Factory.GetBucketName(),
                Prefix = prefix
            };

            var response = await _amazonS3Client.ListObjectsV2Async(request, cancellationToken);
            if (IsSuccess(response))
            {
                var objectIdsSet = keys.Select(key => prefix + key)
                    .ToHashSet();

                return response.S3Objects
                    .Select(s3Object => s3Object.Key)
                    .Where(objectIdsSet.Contains)
                    .Select(key => key.Substring(prefix.Length))
                    .ToHashSet();
            }

            error = response?.HttpStatusCode.ToString() ?? "unknown error";
        }
        catch (Exception ex)
        {
            error = ex.Message;
        }

        _logger.LogS3ListObjectsError(error);
        return DomainErrors.S3ListObjectsError();
    }


    /// <inheritdoc/>
    [TraceMethod]
    public async Task<UnitResult<DomainError>> Save(string prefix, string key, AppFile appFile, CancellationToken cancellationToken)
    {
        string error;
        try
        {
            var request = new PutObjectRequest
            {
                AutoCloseStream = false,
                BucketName = _amazonS3Factory.GetBucketName(),
                ContentType = appFile.ContentMimeType,
                Key = BuildPrefix(prefix) + key,
                InputStream = appFile.Content
            };

            var response = await _amazonS3Client.PutObjectAsync(request, cancellationToken);
            if (IsSuccess(response))
                return UnitResult.Success<DomainError>();

            error = response?.HttpStatusCode.ToString() ?? "unknown error";
        }
        catch (Exception ex)
        {
            error = ex.Message;
        }

        _logger.LogS3UploadObjectError(error);
        return DomainErrors.S3UploadObjectError();
    }


    /// <inheritdoc/>
    [TraceMethod]
    public async Task<Result<AppFile, DomainError>> Get(string prefix, string key, CancellationToken cancellationToken)
    {
        string error;
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = _amazonS3Factory.GetBucketName(),
                Key = BuildPrefix(prefix) + key,
            };

            using var response = await _amazonS3Client.GetObjectAsync(request, cancellationToken);
            if (IsSuccess(response))
            {
                var contentStream = new MemoryStream();
                await response.ResponseStream.CopyToAsync(contentStream, cancellationToken);
                contentStream.Position = 0;

                return new AppFile(contentStream, response.Headers.ContentType);
            }

            error = response?.HttpStatusCode.ToString() ?? "unknown error";
        }
        catch (Exception ex)
        {
            error = ex.Message;
        }

        _logger.LogS3GetObjectError(error);
        return DomainErrors.S3GetObjectError();
    }


    /// <inheritdoc/>
    [TraceMethod]
    public async Task<UnitResult<DomainError>> Delete(string prefix, string key, CancellationToken cancellationToken)
    {
        string error;
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _amazonS3Factory.GetBucketName(),
                Key = BuildPrefix(prefix) + key
            };

            var response = await _amazonS3Client.DeleteObjectAsync(request, cancellationToken);
            if (response is not null && response.HttpStatusCode is System.Net.HttpStatusCode.NoContent)
                return UnitResult.Success<DomainError>();

            error = response?.HttpStatusCode.ToString() ?? "unknown error";
        }
        catch (Exception ex)
        {
            error = ex.Message;
        }

        _logger.LogS3DeleteObjectError(error);
        return DomainErrors.S3DeleteObjectError();
    }

    
    private static string BuildPrefix(string key) 
        => key.TrimEnd('/') + '/';

    
    private static bool IsSuccess(Amazon.Runtime.AmazonWebServiceResponse response)
        => response is not null && response.HttpStatusCode is System.Net.HttpStatusCode.OK;

    
    private readonly AmazonS3Client _amazonS3Client;
    private readonly IAmazonS3Factory _amazonS3Factory;
    private readonly ILogger<S3FileStorage> _logger;
}
