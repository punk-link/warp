﻿using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Warp.WebApp.Attributes;
using Warp.WebApp.Data;
using Warp.WebApp.Extensions;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Entries.Enums;
using Warp.WebApp.Models.Validators;
using Warp.WebApp.Services.Images;
using Warp.WebApp.Services.Infrastructure;

namespace Warp.WebApp.Services.Entries;

public sealed class EntryService : IEntryService
{
    public EntryService(IStringLocalizer<ServerResources> localizer, IDataStorage dataStorage, IImageService imageService, 
        IOpenGraphService openGraphService, IReportService reportService, IUrlService urlService, IViewCountService viewCountService)
    {
        _dataStorage = dataStorage;
        _imageService = imageService;
        _localizer = localizer;
        _openGraphService = openGraphService;
        _reportService = reportService;
        _urlService = urlService;
        _viewCountService = viewCountService;
    }


    [TraceMethod]
    public async Task<Result<Entry, ProblemDetails>> Add(string content, TimeSpan expiresIn, List<Guid> imageIds, EditMode editMode, 
        CancellationToken cancellationToken)
    {
        return await BuildEntry()
            .Bind(Validate)
            .Bind(AttachImages)
            .Bind(BuildOpenGraphMetadata)
            .Tap(AddEntry);


        Result<Entry, ProblemDetails> BuildEntry()
        {
            var now = DateTime.UtcNow;
            var expirationTime = now + expiresIn;
            var formattedText = TextFormatter.Format(content);
            
            return new Entry(Guid.NewGuid(), formattedText, now, expirationTime, editMode);
        }


        async Task<Result<Entry, ProblemDetails>> Validate(Entry entry)
        {
            var validator = new EntryValidator(_localizer, imageIds);
            var validationResult = await validator.ValidateAsync(entry, cancellationToken);
            if (!validationResult.IsValid)
                return validationResult.ToFailure<Entry>(_localizer);

            return entry;
        }


        async Task<Result<Entry, ProblemDetails>> AttachImages(Entry entry)
        {
            var attachedImageIds = await _imageService.Attach(imageIds, cancellationToken);
            var imageUrls = new List<Uri>(attachedImageIds.Count);
            foreach (var imageId in attachedImageIds)
            {
                var url = _urlService.GetImageUrl(entry.Id, in imageId);
                imageUrls.Add(url);
            }

            return entry with { ImageIds = attachedImageIds };
        }


        Result<Entry, ProblemDetails> BuildOpenGraphMetadata(Entry entry)
        {
            var description = _openGraphService.BuildDescription(entry);
            return entry with{ OpenGraphDescription = description };
        }


        async Task AddEntry(Entry entry)
        {
            var entryIdCacheKey = CacheKeyBuilder.BuildEntryCacheKey(entry.Id);
            await _dataStorage.Set(entryIdCacheKey, entry, expiresIn, cancellationToken);
        }
    }


    [TraceMethod]
    public async Task<Result<EntryInfo, ProblemDetails>> Get(Guid entryId, bool isRequestedByCreator = false, CancellationToken cancellationToken = default)
    {
        return await EnsureNotReported()
            .Bind(_ => GetEntry())
            .Bind(GetViews)
            .Bind(GetImageIds)
            .Bind(BuildEntryInfo);


        async Task<Result<DummyObject, ProblemDetails>> EnsureNotReported()
        {
            if (await _reportService.Contains(entryId, cancellationToken))
                return ResultHelper.NotFound<DummyObject>(_localizer);

            return new DummyObject();
        }


        async Task<Result<Entry, ProblemDetails>> GetEntry()
        {
            var entryIdCacheKey = CacheKeyBuilder.BuildEntryCacheKey(entryId);

            var entry = await _dataStorage.TryGet<Entry>(entryIdCacheKey, cancellationToken);
            if (entry.Equals(default))
                return ResultHelper.NotFound<Entry>(_localizer);

            return entry;
        }


        async Task<Result<(Entry, long), ProblemDetails>> GetViews(Entry entry)
        { 
            var viewCount = isRequestedByCreator 
                ? await _viewCountService.Get(entryId, cancellationToken)
                : await _viewCountService.AddAndGet(entryId, cancellationToken);
            
            return (entry, viewCount);
        }


        async Task<Result<(Entry, long, List<Guid>), ProblemDetails>> GetImageIds((Entry Entry, long ViewCount) tuple)
        {
            var imageIds = (await _imageService.GetImageList(tuple.Entry.ImageIds, cancellationToken))
                .Select(image => image.Id)
                .ToList();

            return (tuple.Entry, tuple.ViewCount, imageIds);
        }


        Result<EntryInfo, ProblemDetails> BuildEntryInfo((Entry Entry, long ViewCount, List<Guid> ImageIds) tuple) 
            => new EntryInfo(tuple.Entry, tuple.ViewCount, tuple.ImageIds);
    }


    [TraceMethod]
    public async Task<Result> Remove(Guid entryId, CancellationToken cancellationToken)
    {
        var entryIdCacheKey = CacheKeyBuilder.BuildEntryCacheKey(entryId);
        await _dataStorage.Remove<Entry>(entryIdCacheKey, cancellationToken);

        return Result.Success();
    }


    private readonly IDataStorage _dataStorage;
    private readonly IImageService _imageService;
    private readonly IOpenGraphService _openGraphService;
    private readonly IStringLocalizer<ServerResources> _localizer;
    private readonly IReportService _reportService;
    private readonly IUrlService _urlService;
    private readonly IViewCountService _viewCountService;
}