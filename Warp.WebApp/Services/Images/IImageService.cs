using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;

namespace Warp.WebApp.Services.Images;

public interface IImageService
{
    public Task<Result<List<ImageInfo>, ProblemDetails>> GetAttached(Guid entryId, List<Guid> imageIds, CancellationToken cancellationToken);
    public Task<UnitResult<ProblemDetails>> Remove(Guid entryId, Guid imageId, CancellationToken cancellationToken);
}
