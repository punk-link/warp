using Amazon.S3;

namespace Warp.WebApp.Data.S3;

public interface IAmazonS3
{
    public AmazonS3Client CreateClient();
    public string GetBucketName();
}
