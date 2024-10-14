﻿using Amazon.Runtime.Internal;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using Warp.WebApp.Models;

namespace Warp.WebApp.Data.S3;

public class S3FileStorage : IS3FileStorage
{
    public S3FileStorage(IAmazonS3Fabric amazonS3Fabric)
    {
        _amazonS3Fabric = amazonS3Fabric;
    }

    public async Task Save(ImageInfo imageInfo, CancellationToken cancellationToken)
    {
        var s3Client = _amazonS3Fabric.CreateClient();
        using (var memoryStream = new MemoryStream(imageInfo.Content))
        {
            var request = new PutObjectRequest
            {
                BucketName = _amazonS3Fabric.GetBucketName(),
                Key = imageInfo.Id.ToString(),
                InputStream = memoryStream,
                ContentType = imageInfo.ContentType
            };
            var result = await s3Client.PutObjectAsync(request, cancellationToken);
        }
    }

    public async Task<ImageInfo> Get(Guid key, CancellationToken cancellationToken)
    {
        var s3Client = _amazonS3Fabric.CreateClient();
        var request = new GetObjectRequest
        {
            BucketName = _amazonS3Fabric.GetBucketName(),
            Key = key.ToString(),
        };

        var result = await s3Client.GetObjectAsync(request, cancellationToken);
        if (result != null)
        {
            using (var memoryStream = new MemoryStream())
            {
                result.ResponseStream.CopyTo(memoryStream);
                return new ImageInfo()
                {
                    Id = key,
                    Content = memoryStream.ToArray(),
                    ContentType = result.Headers.ContentType
                };
            }
        }

        else return default;
    }


    private readonly IAmazonS3Fabric _amazonS3Fabric;
}
