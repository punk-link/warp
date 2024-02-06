using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;

namespace Warp.WebApp.Services;

public interface IImageService
{
    Task<Dictionary<string, Guid>> Add(List<IFormFile> files);

    Task Attach(Guid entryId, TimeSpan relativeExpirationTime, List<Guid> imageIds);

    Task<List<ImageEntry>> Get(Guid entryId);

    Task<Result<ImageEntry, ProblemDetails>> Get(Guid entryId, Guid imageId);
}