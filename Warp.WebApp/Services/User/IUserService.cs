using CSharpFunctionalExtensions;
using Warp.WebApp.Models;

namespace Warp.WebApp.Services.User;

public interface IUserService
{
    public Task<Result> AttachEntryToUser(string userId, Entry value, TimeSpan expiresIn, CancellationToken cancellationToken);
    public Task<Result> TryGetUserEntry(string userId, Guid entryId, CancellationToken cancellationToken);
}
