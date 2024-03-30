using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;

namespace Warp.WebApp.Services.Images;

public interface IImageService
{
    public Task<Dictionary<string, Guid>> Add(List<IFormFile> files);
    public Task Add(ImageInfo image);
    public Task Attach(Guid entryId, TimeSpan relativeExpirationTime, List<Guid> imageIds);
    public Task<List<ImageInfo>> Get(Guid entryId);
    public Task<Result<ImageInfo, ProblemDetails>> Get(Guid entryId, Guid imageId);
}