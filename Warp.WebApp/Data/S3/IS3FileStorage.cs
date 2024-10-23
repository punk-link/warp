using Warp.WebApp.Models;

namespace Warp.WebApp.Data.S3;

public interface IS3FileStorage
{
    public Task Save(ImageInfo imageInfo, CancellationToken cancellationToken);

    public Task<ImageInfo> Get(Guid imageId, CancellationToken cancellationToken);

    public Task Delete(Guid imageId, CancellationToken cancellationToken);
}
