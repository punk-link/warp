using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;

namespace Warp.WebApp.Services.User;

public interface IUserService
{
    public Task<Result> AttachEntryToUser(string userIdCacheKey, string entryCacheKey, Entry value, TimeSpan expiresIn, CancellationToken cancellationToken);
    public Task<Entry?> TryGetUserEntry(string userIdCacheKey, Guid entryId, CancellationToken cancellationToken);
}
