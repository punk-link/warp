using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;

namespace Warp.WebApp.Services.Images;

public interface IImageService
{
    public Task<Dictionary<string, Guid>> Add(List<IFormFile> files, CancellationToken cancellationToken);
    public Task<List<Guid>> Attach(Guid entryId, TimeSpan relativeExpirationTime, List<Guid> imageIds, CancellationToken cancellationToken);
    public Task<List<ImageInfo>> Get(Guid entryId, CancellationToken cancellationToken);
    public Task<Result<ImageInfo, ProblemDetails>> Get(Guid entryId, Guid imageId, CancellationToken cancellationToken);
}