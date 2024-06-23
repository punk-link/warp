using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;

namespace Warp.WebApp.Services.Entries;

public interface IEntryPresentationService
{
    public Task<Result<Entry, ProblemDetails>> Add(EntryRequest request, HttpContext httpContext, CancellationToken cancellationToken);
    public Task<Result<EntryInfo, ProblemDetails>> Get(string encodedId, HttpContext httpContext, CancellationToken cancellationToken);
}