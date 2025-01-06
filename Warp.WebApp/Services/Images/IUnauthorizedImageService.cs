using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Files;

namespace Warp.WebApp.Services.Images;

public interface IUnauthorizedImageService
{
    Task<Result<ImageResponse, ProblemDetails>> Add(Guid entryId, AppFile appFile, CancellationToken cancellationToken);
    public Task<Result<Image, ProblemDetails>> Get(Guid entryId, Guid imageId, CancellationToken cancellationToken);
}