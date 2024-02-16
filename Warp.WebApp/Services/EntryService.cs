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
    public EntryService(IDataStorage dataStorage, IImageService imageService, IReportService reportService, IViewCountService viewCountService)
    {
        _dataStorage = dataStorage;
        _imageService = imageService;
        _reportService = reportService;
        _viewCountService = viewCountService;
    }


    public async Task<Result<Guid, ProblemDetails>> Add(string content, TimeSpan expiresIn, List<Guid> imageIds)
    {
        var now = DateTime.UtcNow;
        var entry = new Entry(Guid.NewGuid(), content, now, now + expiresIn);
        
        var validator = new EntryValidator();
        var validationResult = await validator.ValidateAsync(entry);
        if (!validationResult.IsValid)
            return validationResult.ToFailure<Guid>();
        
        var cacheKey = BuildCacheKey(entry.Id);
        var result = await _dataStorage.Set(cacheKey, entry, expiresIn);

        await _imageService.Attach(entry.Id, expiresIn, imageIds);
        
        return result.IsFailure 
            ? result.ToFailure<Guid>() 
            : Result.Success<Guid, ProblemDetails>(entry.Id);
    }
    
    
    public async Task<Result<EntryInfo, ProblemDetails>> Get(Guid id)
    {
        if (_reportService.Contains(id))
            return ResultHelper.NotFound<EntryInfo>();

        var cacheKey = BuildCacheKey(id);
        var entry = await _dataStorage.TryGet<Entry>(cacheKey);
        if (entry is null || entry.Equals(default))
            return ResultHelper.NotFound<EntryInfo>();

        var viewCount = await _viewCountService.AddAndGet(id);
        var imageIds = (await _imageService.Get(id))
            .Select(image => image.Id)
            .ToList();

        return Result.Success<EntryInfo, ProblemDetails>(new EntryInfo
        {
            Entry = entry,
            ImageIds = imageIds,
            ViewCount = viewCount
        });
    }


    private static string BuildCacheKey(Guid id)
        => $"{nameof(Entry)}::{id}";

    
    private readonly IDataStorage _dataStorage;
    private readonly IImageService _imageService;
    private readonly IReportService _reportService;
    private readonly IViewCountService _viewCountService;
}