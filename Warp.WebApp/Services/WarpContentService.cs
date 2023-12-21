using System.Net;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Validators;

namespace Warp.WebApp.Services;

public class WarpContentService : IWarpContentService
{
    public WarpContentService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }


    public Result<Guid, ProblemDetails> Add(WarpContent content)
    {
        var validator = new WarpContentValidator();
        var validationResult = validator.Validate(content);
        if (!validationResult.IsValid)
            return validationResult.ToFailure<Guid>();
        
        content.Id = Guid.NewGuid();
        _memoryCache.Set(content.Id, content, content.ExpiresIn);

        return Result.Success<Guid, ProblemDetails>(content.Id);
    }
    
    
    public Result<WarpContent, ProblemDetails> Get(Guid id)
    {
        if (_memoryCache.TryGetValue(id, out WarpContent? content))
            return Result.Success<WarpContent, ProblemDetails>(content!);
        
        return Result.Failure<WarpContent, ProblemDetails>(ProblemDetailsHelper.Create("Content not found.", HttpStatusCode.NotFound));
    }

    
    private readonly IMemoryCache _memoryCache;
}