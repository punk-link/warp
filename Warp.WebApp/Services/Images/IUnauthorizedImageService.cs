using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;

namespace Warp.WebApp.Services.Images;

public interface IUnauthorizedImageService
{
    public Task<List<ImageResponse>> Add(Guid entryId, List<IFormFile> files, CancellationToken cancellationToken);
    public Task<Result<Image, ProblemDetails>> Get(Guid entryId, Guid imageId, CancellationToken cancellationToken);
}