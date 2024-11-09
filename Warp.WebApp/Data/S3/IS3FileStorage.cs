using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;

namespace Warp.WebApp.Data.S3;

public interface IS3FileStorage
{
    Task<Result<HashSet<Guid>, ProblemDetails>> Contains(Guid entryId, List<Guid> imageIds, CancellationToken cancellationToken);
    public Task Delete(Guid imageId, CancellationToken cancellationToken);
    public Task<Image> Get(Guid imageId, CancellationToken cancellationToken);
    public Task<UnitResult<ProblemDetails>> Save(ImageRequest imageRequest, CancellationToken cancellationToken);
}
