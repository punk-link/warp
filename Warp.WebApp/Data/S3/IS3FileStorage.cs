using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models.Files;

namespace Warp.WebApp.Data.S3;

public interface IS3FileStorage
{
    Task<Result<HashSet<string>, ProblemDetails>> Contains(string prefix, List<string> keys, CancellationToken cancellationToken);
    public Task<UnitResult<ProblemDetails>> Delete(string prefix, string key, CancellationToken cancellationToken);
    public Task<Result<AppFile, ProblemDetails>> Get(string prefix, string key, CancellationToken cancellationToken);
    public Task<UnitResult<ProblemDetails>> Save(string prefix, string key, AppFile appFile, CancellationToken cancellationToken);
}
