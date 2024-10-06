using Amazon.S3.Model;
using Warp.WebApp.Models;

namespace Warp.WebApp.Data.S3;

public interface IFileStorage
{
    public Task SaveFileToStorage(ImageInfo value, CancellationToken cancellationToken);

    public Task<ImageInfo> GetFileFromStorage(Guid key, CancellationToken cancellationToken);
}
