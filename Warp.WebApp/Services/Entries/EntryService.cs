using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Data;
using Warp.WebApp.Extensions;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Validators;
using Warp.WebApp.Services.Images;
using Warp.WebApp.Services.User;

namespace Warp.WebApp.Services.Entries;

public sealed class EntryService : IEntryService
{
    public EntryService(IDataStorage dataStorage, IImageService imageService, IReportService reportService, IViewCountService viewCountService, IUserService userService)
    {
        _dataStorage = dataStorage;
        _imageService = imageService;
        _reportService = reportService;
        _viewCountService = viewCountService;
        _userService = userService;
    }


    public async Task<Result<Guid, ProblemDetails>> Add(Guid entryId, Guid userId, string content, TimeSpan expiresIn, List<Guid> imageIds, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var formattedText = TextFormatter.Format(content);
        var description = OpenGraphService.GetDescription(formattedText);
        entryId = entryId == Guid.Empty ? Guid.NewGuid() : entryId;

        var entry = new Entry(entryId, formattedText, description, now, now + expiresIn);

        var validator = new EntryValidator();
        var validationResult = await validator.ValidateAsync(entry, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToFailure<Guid>();

        var userIdCacheKey = CacheKeyBuilder.BuildListStringCacheKey(userId);
        var entryIdCacheKey = CacheKeyBuilder.BuildEntryCacheKey(entry.Id);
        var result = await _userService.AttachEntryToUser(userIdCacheKey, entryIdCacheKey, entry, expiresIn, cancellationToken);

        await _imageService.Attach(entry.Id, expiresIn, imageIds, cancellationToken);

        return result.IsFailure
            ? result.ToFailure<Guid>()
            : Result.Success<Guid, ProblemDetails>(entry.Id);
    }


    public async Task<Result<EntryInfo, ProblemDetails>> Get(Guid userId, Guid entryId, CancellationToken cancellationToken)
    {
        if (await _reportService.Contains(entryId, cancellationToken))
            return ResultHelper.NotFound<EntryInfo>();

        var entryIdCacheKey = CacheKeyBuilder.BuildEntryCacheKey(entryId);
        var userIdCacheKey = CacheKeyBuilder.BuildListStringCacheKey(userId);

        var entry = userId != Guid.Empty
            ? await _userService.TryGetUserEntry(userIdCacheKey, entryId, cancellationToken)
            : await _dataStorage.TryGet<Entry>(entryIdCacheKey, cancellationToken);

        if (entry.Equals(default))
            return ResultHelper.NotFound<EntryInfo>();

        var viewCount = await _viewCountService.AddAndGet(entryId, cancellationToken);
        var imageIds = (await _imageService.Get(entryId, cancellationToken))
            .Select(image => image.Id)
            .ToList();

        return Result.Success<EntryInfo, ProblemDetails>(new EntryInfo((Entry)entry, viewCount, imageIds));
    }


    public async Task<Result<DummyObject, ProblemDetails>> Remove(Guid id, CancellationToken cancellationToken)
    {
        if (await _reportService.Contains(id, cancellationToken))
            return ResultHelper.NotFound<DummyObject>();

        var cacheKey = CacheKeyBuilder.BuildEntryCacheKey(id);
        await _dataStorage.Remove<EntryInfo>(cacheKey, cancellationToken);
        return Result.Success<DummyObject, ProblemDetails>(DummyObject.Empty);
    }


    private readonly IDataStorage _dataStorage;
    private readonly IImageService _imageService;
    private readonly IReportService _reportService;
    private readonly IViewCountService _viewCountService;
    private readonly IUserService _userService;
}