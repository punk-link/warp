using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Warp.WebApp.Attributes;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models.Files;
using Warp.WebApp.Telemetry.Logging;

namespace Warp.WebApp.Data.S3;

public class S3FileStorage : IS3FileStorage
{
    public S3FileStorage(IStringLocalizer<ServerResources> localizer, IAmazonS3Factory amazonS3Factory, ILoggerFactory loggerFactory)
    {
        _localizer = localizer;
        _logger = loggerFactory.CreateLogger<S3FileStorage>();

        _amazonS3Client = amazonS3Factory.CreateClient();
        _amazonS3Factory = amazonS3Factory;
    }


    [TraceMethod]
    public async Task<Result<HashSet<string>, ProblemDetails>> Contains(string prefix, List<string> keys, CancellationToken cancellationToken)
    {
        prefix = BuildPrefix(prefix);
        var request = new ListObjectsV2Request
        {
            BucketName = _amazonS3Factory.GetBucketName(),
            Prefix = prefix
        };

        string error;
        try
        {
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

        error = string.Format(_localizer["Failed to list objects in S3"], error);
        return Result.Failure<HashSet<string>, ProblemDetails>(ProblemDetailsHelper.CreateServerException(error));
    }


    [TraceMethod]
    public async Task<UnitResult<ProblemDetails>> Save(string prefix, string key, AppFile appFile, CancellationToken cancellationToken)
    {
        var request = new PutObjectRequest
        {
            AutoCloseStream = false,
            BucketName = _amazonS3Factory.GetBucketName(),
            ContentType = appFile.ContentMimeType,
            Key = BuildPrefix(prefix) + key,
            InputStream = appFile.Content
        };

        string error;
        try
        {
            var response = await _amazonS3Client.PutObjectAsync(request, cancellationToken);
            if (IsSuccess(response))
                return UnitResult.Success<ProblemDetails>();

            error = response?.HttpStatusCode.ToString() ?? "unknown error";
        }
        catch (Exception ex)
        {
            error = ex.Message;
        }

        error = string.Format(_localizer["Failed to save an image to S3"], error);
        var problemDetails = ProblemDetailsHelper.CreateServerException(error);

        _logger.LogImageUploadError(error);
        return UnitResult.Failure(problemDetails);
    }


    [TraceMethod]
    public async Task<Result<AppFile, ProblemDetails>> Get(string prefix, string key, CancellationToken cancellationToken)
    {
        var request = new GetObjectRequest
        {
            BucketName = _amazonS3Factory.GetBucketName(),
            Key = BuildPrefix(prefix) + key,
        };

        string error;
        try
        {
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

        error = string.Format(_localizer["Failed to get an image from S3"], error);
        var problemDetails = ProblemDetailsHelper.CreateServerException(error);

        _logger.LogImageDownloadError(error);
        return Result.Failure<AppFile, ProblemDetails>(problemDetails);
    }


    [TraceMethod]
    public async Task<UnitResult<ProblemDetails>> Delete(string prefix, string key, CancellationToken cancellationToken)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = _amazonS3Factory.GetBucketName(),
            Key = BuildPrefix(prefix) + key
        };

        string error;
        try
        {
            var response = await _amazonS3Client.DeleteObjectAsync(request, cancellationToken);
            if (response is not null && response.HttpStatusCode is System.Net.HttpStatusCode.NoContent)
                return UnitResult.Success<ProblemDetails>();

            error = response?.HttpStatusCode.ToString() ?? "unknown error";
        }
        catch (Exception ex)
        {
            error = ex.Message;
        }

        error = string.Format(_localizer["Failed to delete an image from S3"], error);
        var problemDetails = ProblemDetailsHelper.CreateServerException(error);

        _logger.LogImageRemovalError(error);
        return UnitResult.Failure(problemDetails);
    }


    private static string BuildPrefix(string key) 
        => key.TrimEnd('/') + '/';


    private static bool IsSuccess(Amazon.Runtime.AmazonWebServiceResponse response)
        => response is not null && response.HttpStatusCode is System.Net.HttpStatusCode.OK;


    private readonly AmazonS3Client _amazonS3Client;
    private readonly IAmazonS3Factory _amazonS3Factory;
    private readonly IStringLocalizer<ServerResources> _localizer;
    private readonly ILogger<S3FileStorage> _logger;
}
