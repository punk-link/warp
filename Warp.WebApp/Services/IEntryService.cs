using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;

namespace Warp.WebApp.Services;

public interface IEntryService
{
    public Task<Result<Guid, ProblemDetails>> Add(string content, TimeSpan expiresIn, List<Guid> imageIds);
    public Task<Result<EntryInfo, ProblemDetails>> Get(Guid id);
}