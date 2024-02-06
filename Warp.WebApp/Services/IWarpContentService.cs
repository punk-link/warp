using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;

namespace Warp.WebApp.Services;

public interface IWarpContentService
{
    Task<Result<Guid, ProblemDetails>> Add(string content, TimeSpan expiresIn);

    Task<Result<WarpEntry, ProblemDetails>> Get(Guid id);
}