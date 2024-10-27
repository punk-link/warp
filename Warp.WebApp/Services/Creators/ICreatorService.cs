using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Creators;

namespace Warp.WebApp.Services.Creators;

public interface ICreatorService
{
    public Task<Creator> Add(CancellationToken cancellationToken);
    public Task<UnitResult<ProblemDetails>> AttachEntry(Creator creator, EntryInfo entryInfo, CancellationToken cancellationToken);
    public Task<Result<Creator, ProblemDetails>> Get(Guid? creatorId, CancellationToken cancellationToken);
}