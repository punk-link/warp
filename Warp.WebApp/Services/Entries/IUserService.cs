using CSharpFunctionalExtensions;
using Warp.WebApp.Models;

namespace Warp.WebApp.Services.Entries;

public interface IUserService
{
    public Task<Result> Set<Entry>(string userId, Models.Entry value, TimeSpan expiresIn, CancellationToken cancellationToken);
}
