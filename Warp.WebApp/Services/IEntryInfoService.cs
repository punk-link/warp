using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Creators;

namespace Warp.WebApp.Services;
public interface IEntryInfoService
{
    Task<Result<EntryInfo, ProblemDetails>> Add(Creator creator, EntryRequest entryRequest, CancellationToken cancellationToken);
    Task<Result<EntryInfo, ProblemDetails>> Get(Creator creator, Guid entryId, CancellationToken cancellationToken);
    Task<UnitResult<ProblemDetails>> Remove(Creator creator, Guid entryId, CancellationToken cancellationToken);
    Task<UnitResult<ProblemDetails>> RemoveImage(Creator creator, Guid entryId, Guid imageId, CancellationToken cancellationToken);
}