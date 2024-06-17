using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;
using Warp.WebApp.Pages.Shared.Components;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Entries;
using Warp.WebApp.Services.Images;

namespace Warp.WebApp.Pages;

public class EntryModel : BasePageModel
{
    public EntryModel(ILoggerFactory loggerFactory, IEntryService entryService)
        : base(loggerFactory)
    {
        _entryService = entryService;
    }


    public async Task<IActionResult> OnGet(string id, CancellationToken cancellationToken)
    {
        var decodedId = IdCoder.Decode(id);
        if (decodedId == Guid.Empty)
            return RedirectToError(ProblemDetailsHelper.Create("Can't decode a provided ID."));

        var (_, isFailure, entry, problemDetails) = await _entryService.Get(Guid.Empty, decodedId, cancellationToken, true);
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


    private readonly IEntryService _entryService;
}