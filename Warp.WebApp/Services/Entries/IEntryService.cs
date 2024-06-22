using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;

namespace Warp.WebApp.Services.Entries;

public interface IEntryService
{
    public Task<Result<Entry, ProblemDetails>> Add(string content, TimeSpan expiresIn, List<Guid> imageIds, CancellationToken cancellationToken);
    public Task<Result<EntryInfo, ProblemDetails>> Get(Guid entryId, CancellationToken cancellationToken, bool isReceivedForCustomer = false);
    public Task<Result> Remove(Guid entryId, CancellationToken cancellationToken);
}