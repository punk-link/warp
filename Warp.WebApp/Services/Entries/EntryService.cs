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

        var result = await _userService.AttachEntryToUser(userId, entry, expiresIn, cancellationToken);

        await _imageService.Attach(entry.Id, expiresIn, imageIds, cancellationToken);

        return result.IsFailure
            ? result.ToFailure<Guid>()
            : Result.Success<Guid, ProblemDetails>(entry.Id);
    }


    public async Task<Result<EntryInfo, ProblemDetails>> Get(Guid userId, Guid entryId, CancellationToken cancellationToken, bool isReceivedForCustomer = false)
    {
        if (await _reportService.Contains(entryId, cancellationToken))
            return ResultHelper.NotFound<EntryInfo>();

        var entryIdCacheKey = CacheKeyBuilder.BuildEntryCacheKey(entryId);

        var entry = userId != Guid.Empty
            ? await _userService.TryGetUserEntry(userId, entryId, cancellationToken)
            : await _dataStorage.TryGet<Entry>(entryIdCacheKey, cancellationToken);

        if (!entry.HasValue || entry.Value.Equals(default))
            return ResultHelper.NotFound<EntryInfo>();

        var viewCount = isReceivedForCustomer
            ? await _viewCountService.AddAndGet(entryId, cancellationToken)
            : await _viewCountService.Get(entryId, cancellationToken);
                

        var imageIds = (await _imageService.Get(entryId, cancellationToken))
            .Select(image => image.Id)
            .ToList();

        return Result.Success<EntryInfo, ProblemDetails>(new EntryInfo((Entry)entry, viewCount, imageIds));
    }


    public async Task<Result> Remove(Guid userId, Guid entryId, CancellationToken cancellationToken)
    {
        return await _userService.TryToRemoveUserEntry(userId, entryId, cancellationToken);
    }


    private readonly IDataStorage _dataStorage;
    private readonly IImageService _imageService;
    private readonly IReportService _reportService;
    private readonly IViewCountService _viewCountService;
    private readonly IUserService _userService;
}