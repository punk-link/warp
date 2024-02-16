using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Data;
using Warp.WebApp.Extensions;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Validators;

namespace Warp.WebApp.Services;

public class EntryService : IEntryService
{
    public EntryService(IDataStorage dataStorage, IImageService imageService, IReportService reportService)
    {
        _dataStorage = dataStorage;
        _imageService = imageService;
        _reportService = reportService;
    }


    public async Task<Result<Guid, ProblemDetails>> Add(string content, TimeSpan expiresIn, List<Guid> imageIds)
    {
        var now = DateTime.UtcNow;
        var warpEntry = new Entry(Guid.NewGuid(), content, now, now + expiresIn);
        
        var validator = new EntryValidator();
        var validationResult = await validator.ValidateAsync(warpEntry);
        if (!validationResult.IsValid)
            return validationResult.ToFailure<Guid>();
        
        var cacheKey = BuildCacheKey(warpEntry.Id);
        var result = await _dataStorage.Set(cacheKey, warpEntry, expiresIn);
        
        return result.IsFailure 
            ? result.ToFailure<Guid>() 
            : Result.Success<Guid, ProblemDetails>(warpEntry.Id);
    }
    
    
    public async Task<Result<Entry, ProblemDetails>> Get(Guid id)
    {
        if (_reportService.Contains(id))
            return ResultHelper.NotFound<Entry>();

        var cacheKey = BuildCacheKey(id);
        var entry = await _dataStorage.TryGet<Entry>(cacheKey);
        if (entry is not null && !entry.Equals(default))
            return Result.Success<Entry, ProblemDetails>(entry);
        
        return ResultHelper.NotFound<Entry>();
    }


    private static string BuildCacheKey(Guid id)
        => $"{nameof(Entry)}::{id}";

    
    private readonly IDataStorage _dataStorage;
    private readonly IImageService _imageService;
    private readonly IReportService _reportService;
}