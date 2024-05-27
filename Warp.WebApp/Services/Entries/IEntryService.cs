﻿using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;

namespace Warp.WebApp.Services.Entries;

public interface IEntryService
{
    public Task<Result<Guid, ProblemDetails>> Add(Guid userId, string content, TimeSpan expiresIn, List<Guid> imageIds, CancellationToken cancellationToken);
    public Task<Result<EntryInfo, ProblemDetails>> Get(Guid id, CancellationToken cancellationToken);
    public Task<Result<DummyObject, ProblemDetails>> Remove (Guid id, CancellationToken cancellationToken);
}