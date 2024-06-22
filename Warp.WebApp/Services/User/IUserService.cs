﻿using CSharpFunctionalExtensions;
using Warp.WebApp.Models;

namespace Warp.WebApp.Services.User;

public interface IUserService
{
    public Task<Result> AttachEntryToUser(Guid userId, Entry value, TimeSpan expiresIn, CancellationToken cancellationToken);
    public Task<Entry?> TryGetUserEntry(Guid userId, Guid entryId, CancellationToken cancellationToken);
    public Task<Result> TryToRemoveUserEntry(Guid userId, Guid entryId, CancellationToken cancellationToken);
    public Task<bool> IsEntryBelongToUser(Guid userId, Guid entryId, CancellationToken cancellationToken);
}