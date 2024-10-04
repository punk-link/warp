using Amazon.Runtime.Internal;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using Warp.WebApp.Models;

namespace Warp.WebApp.Data.S3;

public class FileStorage
{
    private readonly IConfiguration _configuration;

    public FileStorage(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SaveFileToStorage(ImageInfo value, TimeSpan expiresIn, CancellationToken cancellationToken)
    {
        var s3Client = CreateClient();
        var request = new PutObjectRequest
        {
            BucketName = "warp-webapp-dev",
            Key = value.Id.ToString(),
            InputStream = new MemoryStream(value.Content),
            ContentType = value.ContentType
        };

        var response = await s3Client.PutObjectAsync(request, cancellationToken);
    }

    public async Task<ImageInfo> GetFileFromStorage(Guid key, CancellationToken cancellationToken)
    {
        var s3Client = CreateClient();
        var request = new GetObjectRequest
        {
            BucketName = "warp-webapp-dev",
            Key = key.ToString(),
        };

        var response = await s3Client.GetObjectAsync(request, cancellationToken);
        if (response != null)
        {
            using (var memoryStream = new MemoryStream())
            {
                response.ResponseStream.CopyTo(memoryStream);
                return new ImageInfo()
                {
                    Id = key,
                    Content = memoryStream.ToArray(),
                    ContentType = response.Headers.ContentType
                };
            }
        }

        else return default;
    }
    
    private AmazonS3Client CreateClient()
    {
        var s3Config = GetConfig();
        var s3Client = new AmazonS3Client(_configuration["S3:AccessKey"], _configuration["S3:SecretAccessKey"], s3Config);
        return s3Client;
    }

    private AmazonS3Config GetConfig()
    {
        var s3Config = new AmazonS3Config
        {
            RegionEndpoint = Amazon.RegionEndpoint.EUCentral1
        };
        return s3Config;
    }
}
