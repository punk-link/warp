using Amazon.Runtime.Internal;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using Warp.WebApp.Attributes;
using Warp.WebApp.Models;

namespace Warp.WebApp.Data.S3;

public class S3FileStorage : IS3FileStorage
{
    public S3FileStorage(IAmazonS3Factory amazonS3Factory)
    {
        _amazonS3Client = amazonS3Factory.CreateClient();
        _amazonS3Factory = amazonS3Factory;
    }


    [TraceMethod]
    public async Task Save(ImageInfo imageInfo, CancellationToken cancellationToken)
    {
        using var memoryStream = new MemoryStream(imageInfo.Content);
        var request = new PutObjectRequest
        {
            BucketName = _amazonS3Factory.GetBucketName(),
            Key = imageInfo.Id.ToString(),
            InputStream = memoryStream,
            ContentType = imageInfo.ContentType
        };

        var result = await _amazonS3Client.PutObjectAsync(request, cancellationToken);
    }


    [TraceMethod]
    public async Task<ImageInfo> Get(Guid imageId, CancellationToken cancellationToken)
    {
        var request = new GetObjectRequest
        {
            BucketName = _amazonS3Factory.GetBucketName(),
            Key = imageId.ToString(),
        };

        var result = await _amazonS3Client.GetObjectAsync(request, cancellationToken);

        if (result is null)
            return default;

        using var memoryStream = new MemoryStream();
        result.ResponseStream.CopyTo(memoryStream);

        return new ImageInfo()
        {
            Id = imageId,
            Content = memoryStream.ToArray(),
            ContentType = result.Headers.ContentType
        };
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
}
