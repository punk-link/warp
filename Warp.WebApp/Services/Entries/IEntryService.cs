using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Creators;

namespace Warp.WebApp.Services.Entries;

public interface IEntryService
{
    public Task<Result<Entry, ProblemDetails>> Add(string content, TimeSpan expiresIn, Creator creator, List<Guid> imageIds, CancellationToken cancellationToken);
    public Task<Result<EntryInfo, ProblemDetails>> Get(Guid userId, Guid entryId, CancellationToken cancellationToken, bool isReceivedForCustomer = false);
    public Task<Result> Remove(Guid userId, Guid entryId, CancellationToken cancellationToken);
}