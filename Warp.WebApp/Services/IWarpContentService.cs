using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;

namespace Warp.WebApp.Services;

public interface IWarpContentService
{
    Result<Guid, ProblemDetails> Add(string content, TimeSpan expiresIn);

    Result<WarpEntry, ProblemDetails> Get(Guid id);
}