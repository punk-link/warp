using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Creators;
using Warp.WebApp.Services.Creators;

namespace Warp.WebApp.Services.Entries;

public class EntryPresentationService : IEntryPresentationService
{
    public EntryPresentationService(ICookieService cookieService, ICreatorService creatorService, IEntryService entryService)
    {
        _cookieService = cookieService;
        _creatorService = creatorService;
        _entryService = entryService;
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


    public async Task<Result<EntryInfo, ProblemDetails>> Get(string encodedId, HttpContext httpContext, CancellationToken cancellationToken)
    {
        return await DecodeId()
            .Bind(IsRequestedByCreator)
            .Bind(GetEntry);


        Result<Guid, ProblemDetails> DecodeId()
        {
            var decodedId = IdCoder.Decode(encodedId);
            if (decodedId == Guid.Empty)
                return ProblemDetailsHelper.Create("Can't decode a provided ID.");

            return decodedId;
        }


        async Task<Result<(Guid, bool), ProblemDetails>> IsRequestedByCreator(Guid id)
        {
            var creatorId = _cookieService.GetCreatorId(httpContext);
            if (creatorId is null)
                return (id, false);

            var isEntryBelongsToCreator = await _creatorService.IsEntryBelongsToCreator(creatorId.Value, id, cancellationToken);

            return (id, isEntryBelongsToCreator);
        }


        async Task<Result<EntryInfo, ProblemDetails>> GetEntry((Guid Id, bool IsRequestedByCreator) tuple)
        {
            var result = await _entryService.Get(tuple.Id, tuple.IsRequestedByCreator, cancellationToken);
            if (result.IsFailure)
                return result.Error;

            return result.Value;
        }
    }


    private readonly ICookieService _cookieService;
    private readonly ICreatorService _creatorService;
    private readonly IEntryService _entryService;
}