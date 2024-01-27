using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;

namespace Warp.WebApp.Services;

public interface IImageService
{
    Task<Dictionary<string, Guid>> Add(List<IFormFile> files);

    void Attach(Guid entryId, TimeSpan relativeExpirationTime, List<Guid> imageIds);

    List<ImageEntry> Get(Guid entryId);

    Result<ImageEntry, ProblemDetails> Get(Guid entryId, Guid imageId);
}