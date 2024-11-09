using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Files;

namespace Warp.WebApp.Data.S3;

public interface IS3FileStorage
{
    Task<Result<HashSet<string>, ProblemDetails>> Contains(string prefix, List<string> keys, CancellationToken cancellationToken);
    public Task Delete(Guid imageId, CancellationToken cancellationToken);
    public Task<Result<FileContent, ProblemDetails>> Get(string prefix, string key, CancellationToken cancellationToken);
    public Task<UnitResult<ProblemDetails>> Save(ImageRequest imageRequest, CancellationToken cancellationToken);
}
