using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Data;
using Warp.WebApp.Extensions;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Validators;
using Warp.WebApp.Services.Images;

namespace Warp.WebApp.Services.Entries;

public sealed class EntryService : IEntryService
{
    public EntryService(IDataStorage dataStorage, IImageService imageService, IReportService reportService, IViewCountService viewCountService)
    {
        _dataStorage = dataStorage;
        _imageService = imageService;
        _reportService = reportService;
        _viewCountService = viewCountService;
    }


    public async Task<Result<Guid, ProblemDetails>> Add(Guid userId, string content, TimeSpan expiresIn, List<Guid> imageIds, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var formattedText = TextFormatter.Format(content);
        var description = OpenGraphService.GetDescription(formattedText);
        var entry = new Entry(Guid.NewGuid(), formattedText, description, now, now + expiresIn);

        var validator = new EntryValidator();
        var validationResult = await validator.ValidateAsync(entry, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToFailure<Guid>();

        var cacheKey = BuildCacheKey(entry.Id);
        var result = await _dataStorage.Set(cacheKey, entry, expiresIn, cancellationToken);

        await _imageService.Attach(entry.Id, expiresIn, imageIds, cancellationToken);

        return result.IsFailure
            ? result.ToFailure<Guid>()
            : Result.Success<Guid, ProblemDetails>(entry.Id);
    }


    public async Task<Result<EntryInfo, ProblemDetails>> Get(Guid id, CancellationToken cancellationToken)
    {
        if (await _reportService.Contains(id, cancellationToken))
            return ResultHelper.NotFound<EntryInfo>();

        var cacheKey = BuildCacheKey(id);
        var entry = await _dataStorage.TryGet<Entry>(cacheKey,cancellationToken);
        if (entry.Equals(default))
            return ResultHelper.NotFound<EntryInfo>();

        var viewCount = await _viewCountService.AddAndGet(id, cancellationToken);
        var imageIds = (await _imageService.Get(id, cancellationToken))
            .Select(image => image.Id)
            .ToList();

        return Result.Success<EntryInfo, ProblemDetails>(new EntryInfo(entry, viewCount, imageIds));
    }


    public async Task<Result<DummyObject, ProblemDetails>> Remove(Guid id, CancellationToken cancellationToken)
    {
        if (await _reportService.Contains(id, cancellationToken))
            return ResultHelper.NotFound<DummyObject>();

        var cacheKey = BuildCacheKey(id);
        await _dataStorage.Remove<EntryInfo>(cacheKey, cancellationToken);
        return Result.Success<DummyObject, ProblemDetails>(DummyObject.Empty);
    }


    private static string BuildCacheKey(Guid id)
        => $"{nameof(Entry)}::{id}";


    private readonly IDataStorage _dataStorage;
    private readonly IImageService _imageService;
    private readonly IReportService _reportService;
    private readonly IViewCountService _viewCountService;
}