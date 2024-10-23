using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;

namespace Warp.WebApp.Services.Entries;

public interface IEntryService
{
    public Task<Result<Entry, ProblemDetails>> Add(EntryRequest entryRequest, CancellationToken cancellationToken);
}