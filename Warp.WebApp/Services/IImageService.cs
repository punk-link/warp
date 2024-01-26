using Warp.WebApp.Models;

namespace Warp.WebApp.Services;

public interface IImageService
{
    Task<List<(string, Guid)>> Add(List<IFormFile> files);

    Task<(string, Guid)> Add(IFormFile file);

    void Attach(Guid entryId, DateTimeOffset absoluteExpirationTime, List<Guid> imageIds);

    List<ImageEntry> Get(Guid entryId);
}