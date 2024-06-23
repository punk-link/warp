using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;
using Warp.WebApp.Pages.Shared.Components;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Entries;
using Warp.WebApp.Services.Images;

namespace Warp.WebApp.Pages;

public class EntryModel : BasePageModel
{
    public EntryModel(ILoggerFactory loggerFactory, IEntryPresentationService entryPresentationService)
        : base(loggerFactory)
    {
        _entryPresentationService = entryPresentationService;
    }


    public async Task<IActionResult> OnGet(string id, CancellationToken cancellationToken)
    {
        return await _entryPresentationService.Get(id, HttpContext, cancellationToken)
            .Tap(BuildModel)
            .Tap(AddOpenGraphModel)
            .Finally(result => result.IsSuccess 
                ? Page() 
                : RedirectToError(result.Error));


        void BuildModel(EntryInfo entryInfo)
        {
            Id = id;
            ExpiresIn = new DateTimeOffset(entryInfo.Entry.ExpiresAt).ToUnixTimeMilliseconds();
            TextContent = entryInfo.Entry.Content;
            Description = entryInfo.Entry.Description;
            ViewCount = entryInfo.ViewCount;
            ImageUrls = ImageService.BuildImageUrls(entryInfo.Entry.Id, entryInfo.ImageIds);
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


    private readonly IEntryPresentationService _entryPresentationService;
}