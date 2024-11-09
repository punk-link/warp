using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Sentry.Protocol;
using Warp.WebApp.Attributes;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Files;
using Warp.WebApp.Telemetry.Logging;

namespace Warp.WebApp.Data.S3;

public class S3FileStorage : IS3FileStorage
{
    public S3FileStorage(IAmazonS3Factory amazonS3Factory, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<S3FileStorage>();

        _amazonS3Client = amazonS3Factory.CreateClient();
        _amazonS3Factory = amazonS3Factory;
    }


    [TraceMethod]
    public async Task<Result<HashSet<string>, ProblemDetails>> Contains(string prefix, List<string> keys, CancellationToken cancellationToken)
    {
        prefix = prefix.TrimEnd('/') + '/';
        var request = new ListObjectsV2Request
        {
            BucketName = _amazonS3Factory.GetBucketName(),
            Prefix = prefix
        };

        var response = await _amazonS3Client.ListObjectsV2Async(request, cancellationToken);
        if (response is null || response.HttpStatusCode is not System.Net.HttpStatusCode.OK)
        {
            var error = "Failed to list objects in S3: " + response?.HttpStatusCode.ToString() ?? "unknown error";
            return Result.Failure<HashSet<string>, ProblemDetails>(ProblemDetailsHelper.CreateServerException(error));
        }

        var objectIdsSet = keys.Select(key => prefix + key)
            .ToHashSet();

        var existingObjectIds = response.S3Objects
            .Select(s3Object => s3Object.Key)
            .Where(objectIdsSet.Contains)
            .Select(key => key.Substring(prefix.Length))
            .ToHashSet();

        return existingObjectIds;
    }


    [TraceMethod]
    public async Task<UnitResult<ProblemDetails>> Save(ImageRequest imageRequest, CancellationToken cancellationToken)
    {
        var key = $"{imageRequest.EntryId}/{imageRequest.Id}";

        var request = new PutObjectRequest
        {
            BucketName = _amazonS3Factory.GetBucketName(),
            Key = key,
            InputStream = imageRequest.Content,
            ContentType = imageRequest.ContentType
        };

        var response = await _amazonS3Client.PutObjectAsync(request, cancellationToken);
        if (response is not null && response.HttpStatusCode is System.Net.HttpStatusCode.OK)
            return UnitResult.Success<ProblemDetails>();

        var error = "Failed to save image to S3: " + response?.HttpStatusCode.ToString() ?? "unknown error";
        var problemDetails = ProblemDetailsHelper.CreateServerException(error);

        _logger.LogImageUploadError(error);
        return UnitResult.Failure(problemDetails);
    }


    [TraceMethod]
    public async Task<Result<FileContent, ProblemDetails>> Get(string prefix, string key, CancellationToken cancellationToken)
    {
        prefix = prefix.TrimEnd('/') + '/';

        var request = new GetObjectRequest
        {
            BucketName = _amazonS3Factory.GetBucketName(),
            Key = prefix + key,
        };

        using var response = await _amazonS3Client.GetObjectAsync(request, cancellationToken);
        if (response is not null && response.HttpStatusCode is System.Net.HttpStatusCode.OK)
        {
            var contentStream = new MemoryStream();
            await response.ResponseStream.CopyToAsync(contentStream, cancellationToken);
            contentStream.Position = 0;

            return new FileContent(contentStream, response.Headers.ContentType);
        }

        var error = "Failed to get image from S3: " + response?.HttpStatusCode.ToString() ?? "unknown error";
        var problemDetails = ProblemDetailsHelper.CreateServerException(error);

        _logger.LogImageDownloadError(error);
        return Result.Failure<FileContent, ProblemDetails>(problemDetails);
    }


    [TraceMethod]
    public async Task Delete(Guid imageId, CancellationToken cancellationToken)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = _amazonS3Factory.GetBucketName(),
            Key = imageId.ToString()
        };

        await _amazonS3Client.DeleteObjectAsync(request, cancellationToken);
    }


    private readonly AmazonS3Client _amazonS3Client;
    private readonly IAmazonS3Factory _amazonS3Factory;
    private readonly ILogger<S3FileStorage> _logger;
}
