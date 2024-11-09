using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;

namespace Warp.WebApp.Services.Images;

public interface IImageService
{
    public Task<List<ImageResponse>> Add(Guid entryId, List<IFormFile> files, CancellationToken cancellationToken);
    public Task<Result<Image, ProblemDetails>> Get(Guid entryId, Guid imageId, CancellationToken cancellationToken);
    Task<Result<List<ImageInfo>, ProblemDetails>> GetAttached(Guid entryId, List<Guid> imageIds, CancellationToken cancellationToken);
    public Task Remove(Guid imageId, CancellationToken cancellationToken);
}