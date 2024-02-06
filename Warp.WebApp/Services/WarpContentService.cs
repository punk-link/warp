using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Data;
using Warp.WebApp.Extensions;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Validators;

namespace Warp.WebApp.Services;

public class WarpContentService : IWarpContentService
{
    public WarpContentService(IDataStorage dataStorage, IReportService reportService)
    {
        _dataStorage = dataStorage;
        _reportService = reportService;
    }


    public async Task<Result<Guid, ProblemDetails>> Add(string content, TimeSpan expiresIn)
    {
        var now = DateTime.UtcNow;
        var warpEntry = new WarpEntry(Guid.NewGuid(), content, now, now + expiresIn);
        
        var validator = new WarpEntryValidator();
        var validationResult = await validator.ValidateAsync(warpEntry);
        if (!validationResult.IsValid)
            return validationResult.ToFailure<Guid>();
        
        var cacheKey = BuildCacheKey(warpEntry.Id);
        var result = await _dataStorage.Set(cacheKey, warpEntry, expiresIn);
        
        return result.IsFailure 
            ? result.ToFailure<Guid>() 
            : Result.Success<Guid, ProblemDetails>(warpEntry.Id);
    }
    
    
    public async Task<Result<WarpEntry, ProblemDetails>> Get(Guid id)
    {
        if (_reportService.Contains(id))
            return ResultHelper.NotFound<WarpEntry>();

        var cacheKey = BuildCacheKey(id);
        var entry = await _dataStorage.TryGet<WarpEntry>(cacheKey);
        if (entry is not null && !entry.Equals(default))
            return Result.Success<WarpEntry, ProblemDetails>(entry);
        
        return ResultHelper.NotFound<WarpEntry>();
    }


    private static string BuildCacheKey(Guid id)
        => $"{nameof(WarpEntry)}::{id}";

    
    private readonly IDataStorage _dataStorage;
    private readonly IReportService _reportService;
}