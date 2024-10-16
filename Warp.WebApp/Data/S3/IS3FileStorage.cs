using Amazon.S3.Model;
using Warp.WebApp.Models;

namespace Warp.WebApp.Data.S3;

public interface IS3FileStorage
{
    public Task Save(ImageInfo value, CancellationToken cancellationToken);

    public Task<ImageInfo> Get(Guid key, CancellationToken cancellationToken);
}
