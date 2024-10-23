using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;
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


    private readonly ICookieService _cookieService;
    private readonly ICreatorService _creatorService;
    private readonly IEntryService _entryService;
    private readonly IStringLocalizer<ServerResources> _localizer;
}