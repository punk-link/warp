using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;

namespace Warp.WebApp.Services.Entries;

public interface IEntryService
{
    public Task<Result<Guid, ProblemDetails>> Add(Guid userId, string content, TimeSpan expiresIn, List<Guid> imageIds, CancellationToken cancellationToken);
    public Task<Result<EntryInfo, ProblemDetails>> Get(Guid userId, Guid entryId, CancellationToken cancellationToken, bool isReceivedForCustomer = false, Guid? customerGuid = null);
    public Task<Result> Remove(Guid userId, Guid entryId, CancellationToken cancellationToken);
}