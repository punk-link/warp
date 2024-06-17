using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;
using Warp.WebApp.Pages.Shared.Components;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Entries;
using Warp.WebApp.Services.Images;
using Warp.WebApp.Services.User;

namespace Warp.WebApp.Pages;

public class PreviewModel : BasePageModel
{
    public PreviewModel(ILoggerFactory loggerFactory, IEntryService previewEntryService) : base(loggerFactory)
    {
        _entryService = previewEntryService;
    }


    public async Task<IActionResult> OnGet(string id, CancellationToken cancellationToken)
    {
        // TODO: add localization
        var decodedId = IdCoder.Decode(id);
        if (decodedId == Guid.Empty)
            return RedirectToError(ProblemDetailsHelper.Create("Can't decode a provided ID."));

        var claim = CookieService.GetClaim(HttpContext);
        if (claim is null)
            return RedirectToError(ProblemDetailsHelper.Create("Can`t open preview page cause of no permission."));

        var userGuid = Guid.Parse(claim.Value);
        var (_, isFailure, entry, problemDetails) = await _entryService.Get(userGuid, decodedId, cancellationToken);
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
        var claim = CookieService.GetClaim(HttpContext);
        if (claim is null)
            return RedirectToError(ProblemDetailsHelper.Create("Can`t copy entry cause of no permission."));

        var decodedId = IdCoder.Decode(id);
        var userGuid = Guid.Parse(claim.Value);
        var (_, isGetFailure, entryGet, problemDetailsGet) = await _entryService.Get(userGuid, decodedId, cancellationToken);
        if (isGetFailure)
            return RedirectToError(problemDetailsGet);

        var (_, isAddFailure, newEntryId, problemDetailsAdd) = await _entryService.Add(userGuid, entryGet.Entry.Content, entryGet.Entry.ExpiresAt - entryGet.Entry.CreatedAt, entryGet.ImageIds, cancellationToken);
        if (isAddFailure)
            return RedirectToError(problemDetailsAdd);

        return RedirectToPage("./Index", new { id = IdCoder.Encode(newEntryId) });
    }


    public OpenGraphModel OpenGraphModel { get; set; } = default!;

    public long ExpiresIn { get; set; }
    public string Id { get; set; } = default!;
    public List<string> ImageUrls { get; set; } = [];
    public string TextContent { get; set; } = string.Empty;


    private readonly IEntryService _entryService;
}