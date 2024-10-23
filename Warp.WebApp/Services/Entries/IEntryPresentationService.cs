using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;

namespace Warp.WebApp.Services.Entries;

public interface IEntryPresentationService
{
    public Task<Result<Entry, ProblemDetails>> Copy(string encodedId, HttpContext httpContext, CancellationToken cancellationToken);
}