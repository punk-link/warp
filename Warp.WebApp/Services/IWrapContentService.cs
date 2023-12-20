using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;

namespace Warp.WebApp.Services;

public interface IWrapContentService
{
    Result<Guid, ProblemDetails> Add(WarpContent content);

    Result<WarpContent> Get(Guid id);
}