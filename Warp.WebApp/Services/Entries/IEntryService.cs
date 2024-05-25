using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;

namespace Warp.WebApp.Services.Entries;

public interface IEntryService
{
    public Task<Result<Guid, ProblemDetails>> Add(string content, TimeSpan expiresIn, List<Guid> imageIds, CancellationToken cancellationToken);
    public Task<Result<EntryInfo, ProblemDetails>> Get(Guid id, CancellationToken cancellationToken);
    public Task<Result<DummyObject, ProblemDetails>> Delete (Guid id, CancellationToken cancellationToken);
}