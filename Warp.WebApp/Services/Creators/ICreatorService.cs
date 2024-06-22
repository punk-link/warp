using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Creators;

namespace Warp.WebApp.Services.Creators;

public interface ICreatorService
{
    public Task<Result<Creator, ProblemDetails>> Add(CancellationToken cancellationToken);
    public Task<Result<DummyObject, ProblemDetails>> AttachEntry(Creator creator, Entry entry, TimeSpan expiresIn, CancellationToken cancellationToken);
    public Task<Creator?> Get(Guid? creatorId, CancellationToken cancellationToken);
    public Task<Result<Creator, ProblemDetails>> GetOrAdd(Guid? creatorId, CancellationToken cancellationToken);
    public Task<Result<bool>> IsEntryBelongsToCreator(Guid creatorId, Guid entryId, CancellationToken cancellationToken);
    public Task<Result<bool>> IsEntryBelongsToCreator(Creator creator, Guid entryId, CancellationToken cancellationToken);
}