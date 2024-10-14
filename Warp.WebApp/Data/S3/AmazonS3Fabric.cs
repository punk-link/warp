using Amazon.S3;

namespace Warp.WebApp.Data.S3;

public class AmazonS3Fabric : IAmazonS3Fabric
{
    public AmazonS3Fabric(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public AmazonS3Client CreateClient()
    {
        var s3Config = GetConfig();
        var s3Client = new AmazonS3Client(_configuration["S3:AccessKey"], _configuration["S3:SecretAccessKey"], s3Config);
        return s3Client;
    }

    public string GetBucketName()
    {
        return _configuration["S3:BucketName"];
    }

    private AmazonS3Config GetConfig()
    {
        var s3Config = new AmazonS3Config
        {
            RegionEndpoint = Amazon.RegionEndpoint.EUCentral1
        };
        return s3Config;
    }

    private readonly IConfiguration _configuration;
}
