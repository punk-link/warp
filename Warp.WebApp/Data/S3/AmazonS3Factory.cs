using Amazon.S3;
using Microsoft.Extensions.Options;

namespace Warp.WebApp.Data.S3;

public class AmazonS3Factory : IAmazonS3Factory
{
    public AmazonS3Factory(IOptions<S3Options> options)
    {
        _options = options;
    }


    public AmazonS3Client CreateClient()
    {
        var s3Config = GetConfig();
        return new AmazonS3Client(_options.Value.AccessKey, _options.Value.SecretAccessKey, s3Config);
    }


    public string GetBucketName()
        => _options.Value.BucketName;


    private AmazonS3Config GetConfig()
    {
        if (!string.IsNullOrWhiteSpace(_options.Value.ServiceUrl))
            return new AmazonS3Config
            {
                ForcePathStyle = _options.Value.ForcePathStyle,
                ServiceURL = _options.Value.ServiceUrl
            };

        return new AmazonS3Config
        {
            ForcePathStyle = _options.Value.ForcePathStyle,
            RegionEndpoint = Amazon.RegionEndpoint.EUCentral1
        };
    }


    private readonly IOptions<S3Options> _options;
}
