using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;
using Warp.WebApp.Pages.Shared.Components;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Creators;
using Warp.WebApp.Services.Entries;
using Warp.WebApp.Services.Images;

namespace Warp.WebApp.Pages;

public class EntryModel : BasePageModel
{
    public EntryModel(ILoggerFactory loggerFactory, ICookieService cookieService, ICreatorService creatorService, IEntryService entryService)
        : base(loggerFactory)
    {
        _cookieService = cookieService;
        _creatorService = creatorService;
        _entryService = entryService;
    }


    public async Task<IActionResult> OnGet(string id, CancellationToken cancellationToken)
    {
        var decodedId = IdCoder.Decode(id);
        if (decodedId == Guid.Empty)
            return RedirectToError(ProblemDetailsHelper.Create("Can't decode a provided ID."));

        var isRequestedByCreator = false;
        var creatorId = _cookieService.GetCreatorId(HttpContext);
        if (creatorId is not null)
        {
            var isEntryBelongsToCreatorResult = await _creatorService.IsEntryBelongsToCreator(creatorId.Value, decodedId, cancellationToken);
            if (isEntryBelongsToCreatorResult.IsSuccess)
                isRequestedByCreator = isEntryBelongsToCreatorResult.Value;
        }

        var (_, isFailure, entry, problemDetails) = await _entryService.Get(decodedId, isRequestedByCreator, cancellationToken);
        if (isFailure)
            return RedirectToError(problemDetails);

        Result.Success()
            .Tap(() => BuildModel(id, entry))
            .Tap(AddOpenGraphModel);

        return Page();


        void BuildModel(string entryId, EntryInfo entryInfo)
        {
            Id = entryId;
            ExpiresIn = new DateTimeOffset(entryInfo.Entry.ExpiresAt).ToUnixTimeMilliseconds();
            TextContent = entryInfo.Entry.Content;
            Description = entryInfo.Entry.Description;
            ViewCount = entryInfo.ViewCount;
            ImageUrls = ImageService.BuildImageUrls(decodedId, entryInfo.ImageIds);
        }


        void AddOpenGraphModel()
        {
            OpenGraphModel = OpenGraphService.GetModel(TextContent, ImageUrls);
        }
    }


    public OpenGraphModel OpenGraphModel { get; set; } = default!;


    public long ExpiresIn { get; set; }
    public string Id { get; set; } = default!;
    public List<string> ImageUrls { get; set; } = [];
    public string TextContent { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public long ViewCount { get; set; } = 1;


    private readonly ICookieService _cookieService;
    private readonly ICreatorService _creatorService;
    private readonly IEntryService _entryService;
}