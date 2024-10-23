using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;

namespace Warp.WebApp.Services.Images;

public interface IImageService
{
    public Task<Dictionary<string, Guid>> Add(List<IFormFile> files, CancellationToken cancellationToken);
    public Task<List<Guid>> Attach(List<Guid> imageIds, CancellationToken cancellationToken);
    public Task<List<ImageInfo>> GetImageList(List<Guid> imageIds, CancellationToken cancellationToken);
    public Task<Result<ImageInfo, ProblemDetails>> Get(Guid imageId, CancellationToken cancellationToken);
    public Task Remove(Guid imageId, CancellationToken cancellationToken);
}