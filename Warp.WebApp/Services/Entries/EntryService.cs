using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sentry;
using System.Threading;
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
    public EntryService(IStringLocalizer<ServerResources> localizer, IDataStorage dataStorage, IImageService imageService, IReportService reportService,
        IViewCountService viewCountService, IUserService userService)
    {
        _dataStorage = dataStorage;
        _imageService = imageService;
        _localizer = localizer;
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

        var validator = new EntryValidator(_localizer);
        var validationResult = await validator.ValidateAsync(entry, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToFailure<Guid>(_localizer);

        var result = await _userService.AttachEntryToUser(userId, entry, expiresIn, cancellationToken);

        await _imageService.Attach(entry.Id, expiresIn, imageIds, cancellationToken);

        return result.IsFailure
            ? result.ToFailure<Guid>()
            : Result.Success<Guid, ProblemDetails>(entry.Id);
    }


    public async Task<Result<EntryInfo, ProblemDetails>> Get(Guid userId, Guid entryId, bool isReceivedForCustomer = false, Guid? customerId = null, CancellationToken cancellationToken = default)
    {
        if (await _reportService.Contains(entryId, cancellationToken))
            return ResultHelper.NotFound<EntryInfo>(_localizer);

        var entryIdCacheKey = CacheKeyBuilder.BuildEntryCacheKey(entryId);

        var entry = userId != Guid.Empty
            ? await _userService.TryGetUserEntry(userId, entryId, cancellationToken)
            : await _dataStorage.TryGet<Entry>(entryIdCacheKey, cancellationToken);

        if (!entry.HasValue || entry.Value.Equals(default))
            return ResultHelper.NotFound<EntryInfo>(_localizer);

        var viewCount = await GetViewCount(entryId, cancellationToken, isReceivedForCustomer, customerId);

        var imageIds = (await _imageService.Get(entryId, cancellationToken))
            .Select(image => image.Id)
            .ToList();

        return Result.Success<EntryInfo, ProblemDetails>(new EntryInfo((Entry)entry, viewCount, imageIds));
    }


    public async Task<Result> Remove(Guid userId, Guid entryId, CancellationToken cancellationToken)
        => await _userService.TryToRemoveUserEntry(userId, entryId, cancellationToken);


    private async Task<long> GetViewCount(Guid entryId, CancellationToken cancellationToken, bool isReceivedForCustomer, Guid? customerId)
    {
        return isReceivedForCustomer && !await _userService.IsEntryBelongToUser(customerId.GetValueOrDefault(), entryId, cancellationToken)
            ? await _viewCountService.AddAndGet(entryId, cancellationToken)
            : await _viewCountService.Get(entryId, cancellationToken);
    }


    private readonly IDataStorage _dataStorage;
    private readonly IImageService _imageService;
    private readonly IStringLocalizer<ServerResources> _localizer;
    private readonly IReportService _reportService;
    private readonly IViewCountService _viewCountService;
    private readonly IUserService _userService;
}