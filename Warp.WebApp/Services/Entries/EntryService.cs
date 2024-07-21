using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Warp.WebApp.Data;
using Warp.WebApp.Extensions;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Entries.Enums;
using Warp.WebApp.Models.Validators;
using Warp.WebApp.Services.Images;

namespace Warp.WebApp.Services.Entries;

public sealed class EntryService : IEntryService
{
    public EntryService(IStringLocalizer<ServerResources> localizer, IDataStorage dataStorage, IImageService imageService, IReportService reportService,
        IViewCountService viewCountService)
    {
        _dataStorage = dataStorage;
        _imageService = imageService;
        _localizer = localizer;
        _reportService = reportService;
        _viewCountService = viewCountService;
    }


    public async Task<Result<Entry, ProblemDetails>> Add(string content, TimeSpan expiresIn, List<Guid> imageIds, EditMode editMode, 
        CancellationToken cancellationToken)
    {
        var entry = BuildEntry();

        var validator = new EntryValidator(_localizer, imageIds);
        var validationResult = await validator.ValidateAsync(entry, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToFailure<Entry>(_localizer);

        await _imageService.Attach(entry.Id, expiresIn, imageIds, cancellationToken);

        var entryIdCacheKey = CacheKeyBuilder.BuildEntryCacheKey(entry.Id);
        await _dataStorage.Set(entryIdCacheKey, entry, expiresIn, cancellationToken);

        return entry;


        Entry BuildEntry()
        {
            var now = DateTime.UtcNow;
            var formattedText = TextFormatter.Format(content);
            var description = OpenGraphService.GetDescription(formattedText);
            
            return new Entry(Guid.NewGuid(), formattedText, description, now, now + expiresIn, editMode);
        }
    }


    public async Task<Result<EntryInfo, ProblemDetails>> Get(Guid entryId, bool isRequestedByCreator = false, CancellationToken cancellationToken = default)
    {
        if (await _reportService.Contains(entryId, cancellationToken))
            return ResultHelper.NotFound<EntryInfo>(_localizer);

        var entryIdCacheKey = CacheKeyBuilder.BuildEntryCacheKey(entryId);

        var entry = await _dataStorage.TryGet<Entry>(entryIdCacheKey, cancellationToken);
        if (entry.Equals(default))
            return ResultHelper.NotFound<EntryInfo>(_localizer);

        var viewCount = await GetViewCount(entryId, isRequestedByCreator, cancellationToken);


        var imageIds = (await _imageService.Get(entryId, cancellationToken))
            .Select(image => image.Id)
            .ToList();

        return Result.Success<EntryInfo, ProblemDetails>(new EntryInfo(entry, viewCount, imageIds));
    }


    public async Task<Result> Remove(Guid entryId, CancellationToken cancellationToken)
    {
        var entryIdCacheKey = CacheKeyBuilder.BuildEntryCacheKey(entryId);
        await _dataStorage.Remove<Entry>(entryIdCacheKey, cancellationToken);

        return Result.Success();
    }


    private async Task<long> GetViewCount(Guid entryId, bool isRequestedByCreator, CancellationToken cancellationToken)
    {
        return isRequestedByCreator 
            ? await _viewCountService.Get(entryId, cancellationToken)
            : await _viewCountService.AddAndGet(entryId, cancellationToken);
    }


    private readonly IDataStorage _dataStorage;
    private readonly IImageService _imageService;
    private readonly IStringLocalizer<ServerResources> _localizer;
    private readonly IReportService _reportService;
    private readonly IViewCountService _viewCountService;
}