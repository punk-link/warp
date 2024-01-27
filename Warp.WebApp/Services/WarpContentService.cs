using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Validators;

namespace Warp.WebApp.Services;

public class WarpContentService : IWarpContentService
{
    public WarpContentService(IMemoryCache memoryCache, IReportService reportService)
    {
        _memoryCache = memoryCache;
        _reportService = reportService;
    }


    public Result<Guid, ProblemDetails> Add(string content, TimeSpan expiresIn)
    {
        var now = DateTime.UtcNow;
        var warpEntry = new WarpEntry(Guid.NewGuid(), content, now, now + expiresIn);
        
        var validator = new WarpEntryValidator();
        var validationResult = validator.Validate(warpEntry);
        if (!validationResult.IsValid)
            return validationResult.ToFailure<Guid>();
        
        var cacheKey = BuildCacheKey(warpEntry.Id);
        _memoryCache.Set(cacheKey, warpEntry, expiresIn);

        return Result.Success<Guid, ProblemDetails>(warpEntry.Id);
    }
    
    
    public Result<WarpEntry, ProblemDetails> Get(Guid id)
    {
        if (_reportService.Contains(id))
            return ResultHelper.NotFound<WarpEntry>();

        var cacheKey = BuildCacheKey(id);
        if (_memoryCache.TryGetValue(cacheKey, out WarpEntry? entry))
            return Result.Success<WarpEntry, ProblemDetails>(entry!);
        
        return ResultHelper.NotFound<WarpEntry>();
    }


    private static string BuildCacheKey(Guid id)
        => $"{nameof(WarpEntry)}::{id}";

    
    private readonly IMemoryCache _memoryCache;
    private readonly IReportService _reportService;
}