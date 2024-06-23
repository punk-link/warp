using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Creators;
using Warp.WebApp.Services.Creators;

namespace Warp.WebApp.Services.Entries;

public class EntryPresentationService : IEntryPresentationService
{
    public EntryPresentationService(IStringLocalizer<ServerResources> localizer, ICookieService cookieService, ICreatorService creatorService,
        IEntryService entryService)
    {
        _cookieService = cookieService;
        _creatorService = creatorService;
        _entryService = entryService;
        _localizer = localizer;
    }


    public async Task<Result<Entry, ProblemDetails>> Add(EntryRequest request, HttpContext httpContext, CancellationToken cancellationToken)
    {
        return await GetCreator()
            .Bind(AddEntry)
            .Bind(Attach)
            .Tap(SetCookie)
            .Map(tuple => tuple.Entry);


        async Task<Result<Creator, ProblemDetails>> GetCreator()
        {
            var creatorId = _cookieService.GetCreatorId(httpContext);
            var creatorResult = await _creatorService.GetOrAdd(creatorId, cancellationToken);
            if (creatorResult.IsFailure)
                return creatorResult.Error;

            return creatorResult.Value;
        }


        async Task<Result<(Creator Creator, Entry Entry), ProblemDetails>> AddEntry(Creator creator)
        {
            var entryResult = await _entryService.Add(request.TextContent, request.ExpiresIn, request.ImageIds, cancellationToken);
            if (entryResult.IsFailure)
                return entryResult.Error;

            return (creator, entryResult.Value);
        }


        async Task<Result<(Creator Creator, Entry Entry), ProblemDetails>> Attach((Creator Creator, Entry Entry) tuple)
        {
            var attachResult = await _creatorService.AttachEntry(tuple.Creator, tuple.Entry, request.ExpiresIn, cancellationToken);
            if (attachResult.IsFailure)
                return attachResult.Error;

            return tuple;
        }


        async Task SetCookie((Creator Creator, Entry Entry) tuple)
            => await _cookieService.Set(httpContext, tuple.Creator.Id);
    }


    public async Task<Result<Entry, ProblemDetails>> Copy(string encodedId, HttpContext httpContext, CancellationToken cancellationToken)
    {
        return await Modify(encodedId, httpContext, cancellationToken)
            .Map(info =>
            {
                var expiresIn = info.Entry.ExpiresAt - info.Entry.CreatedAt;
                return new EntryRequest
                {
                    TextContent = info.Entry.Content,
                    ExpiresIn = expiresIn,
                    ImageIds = [] // TODO: add image ids when the feature is implemented
                };
            })
            .Bind(async request => await Add(request, httpContext, cancellationToken));
    }


    public async Task<Result<EntryInfo, ProblemDetails>> Get(string encodedId, HttpContext httpContext, CancellationToken cancellationToken)
    {
        return await DecodeId(encodedId)
            .Bind(IsRequestedByCreator)
            .Bind(GetEntryLocally);


        async Task<Result<(Guid, bool), ProblemDetails>> IsRequestedByCreator(Guid id)
        {
            var creatorId = _cookieService.GetCreatorId(httpContext);
            if (creatorId is null)
                return (id, false);

            var isEntryBelongsToCreator = await _creatorService.EntryBelongsToCreator(creatorId.Value, id, cancellationToken);

            return (id, isEntryBelongsToCreator);
        }


        async Task<Result<EntryInfo, ProblemDetails>> GetEntryLocally((Guid Id, bool IsRequestedByCreator) tuple)
            => await GetEntry(tuple.Id, tuple.IsRequestedByCreator, cancellationToken);
    }


    public async Task<Result<EntryInfo, ProblemDetails>> Modify(string encodedId, HttpContext httpContext, CancellationToken cancellationToken)
    {
        return await DecodeId(encodedId)
            .Bind(BelongsToCreator)
            .Bind(async id => await GetEntry(id, true, cancellationToken));


        async Task<Result<Guid, ProblemDetails>> BelongsToCreator(Guid entryId)
        {
            var creatorId = _cookieService.GetCreatorId(httpContext);
            if (creatorId is null)
                return ProblemDetailsHelper.Create(_localizer["NoPreviewPermissionErrorMessage"]);

            var entryBelongsToCreator = await _creatorService.EntryBelongsToCreator(creatorId.Value, entryId, cancellationToken);
            if (!entryBelongsToCreator)
                return ProblemDetailsHelper.Create(_localizer["NoPreviewPermissionErrorMessage"]);

            return entryId;
        }
    }


    private Result<Guid, ProblemDetails> DecodeId(string encodedId)
    {
        var decodedId = IdCoder.Decode(encodedId);
        if (decodedId == Guid.Empty)
            return ProblemDetailsHelper.Create(_localizer["IdDecodingErrorMessage"]);

        return decodedId;
    }


    private async Task<Result<EntryInfo, ProblemDetails>> GetEntry(Guid id, bool requestsByCreator, CancellationToken cancellationToken)
    {
        var result = await _entryService.Get(id, requestsByCreator, cancellationToken);
        if (result.IsFailure)
            return result.Error;

        return result.Value;
    }


    private readonly ICookieService _cookieService;
    private readonly ICreatorService _creatorService;
    private readonly IEntryService _entryService;
    private readonly IStringLocalizer<ServerResources> _localizer;
}