using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;
using Warp.WebApp.Pages.Shared.Components;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Creators;
using Warp.WebApp.Services.Entries;
using Warp.WebApp.Services.Images;

namespace Warp.WebApp.Pages;

public class PreviewModel : BasePageModel
{
    public PreviewModel(ILoggerFactory loggerFactory, IStringLocalizer<ServerResources> localizer, ICookieService cookieService,
        ICreatorService creatorService, IEntryService entryService) : base(loggerFactory)
    {
        _cookieService = cookieService;
        _creatorService = creatorService;
        _entryService = entryService;
        _localizer = localizer;
    }


    public async Task<IActionResult> OnGet(string id, CancellationToken cancellationToken)
    {
        var decodedId = IdCoder.Decode(id);
        if (decodedId == Guid.Empty)
            return RedirectToError(ProblemDetailsHelper.Create(_localizer["IdDecodingErrorMessage"]));

        var creatorId = _cookieService.GetCreatorId(HttpContext);
        var creator = await _creatorService.Get(creatorId, cancellationToken);
        if (creator is null)
            return RedirectToError(ProblemDetailsHelper.Create(_localizer["NoPreviewPermissionErrorMessage"]));

        var (_, isFailure, entry, problemDetails) = await _entryService.Get(decodedId, cancellationToken: cancellationToken);
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
            ImageUrls = ImageService.BuildImageUrls(decodedId, entryInfo.ImageIds);
        }


        void AddOpenGraphModel()
        {
            OpenGraphModel = OpenGraphService.GetModel(TextContent, ImageUrls);
        }
    }


    public async Task<IActionResult> OnPostCopy(string id, CancellationToken cancellationToken)
    {
        var decodedId = IdCoder.Decode(id);
        if (decodedId == Guid.Empty)
            return RedirectToError(ProblemDetailsHelper.Create(_localizer["IdDecodingErrorMessage"]));
        
        var creatorId = _cookieService.GetCreatorId(HttpContext);
        var creator = await _creatorService.Get(creatorId, cancellationToken);
        if (creator is null)
            return RedirectToError(ProblemDetailsHelper.Create(_localizer["NoPreviewPermissionErrorMessage"]));

        var isEntryBelongsToCreator = await _creatorService.IsEntryBelongsToCreator(creator.Value, decodedId, cancellationToken);
        if (!isEntryBelongsToCreator)
            return RedirectToError(ProblemDetailsHelper.Create(_localizer["NoPreviewPermissionErrorMessage"]));

        var (_, isGetFailure, entryGet, problemDetailsGet) = await _entryService.Get(decodedId, cancellationToken: cancellationToken);
        if (isGetFailure)
            return RedirectToError(problemDetailsGet);

        var expiresIn = entryGet.Entry.ExpiresAt - entryGet.Entry.CreatedAt;
        var (_, isAddFailure, newEntry, problemDetailsAdd) = await _entryService.Add(entryGet.Entry.Content, expiresIn, entryGet.ImageIds, cancellationToken);
        if (isAddFailure)
            return RedirectToError(problemDetailsAdd);

        await _creatorService.AttachEntry(creator.Value, newEntry, expiresIn, cancellationToken);

        return RedirectToPage("./Index", new { id = IdCoder.Encode(newEntry.Id) });
    }


    public OpenGraphModel OpenGraphModel { get; set; } = default!;

    public long ExpiresIn { get; set; }
    public string Id { get; set; } = default!;
    public List<string> ImageUrls { get; set; } = [];
    public string TextContent { get; set; } = string.Empty;

    
    private readonly ICookieService _cookieService;
    private readonly ICreatorService _creatorService;
    private readonly IEntryService _entryService;
    private readonly IStringLocalizer<ServerResources> _localizer;
}