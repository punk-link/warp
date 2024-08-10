using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;
using Warp.WebApp.Pages.Shared.Components;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Entries;

namespace Warp.WebApp.Pages;

public class PreviewModel : BasePageModel
{
    public PreviewModel(ILoggerFactory loggerFactory, IEntryPresentationService entryPresentationService)
        : base(loggerFactory)
    {
        _entryPresentationService = entryPresentationService;
    }


    public async Task<IActionResult> OnGet(string id, CancellationToken cancellationToken)
    {
        return await _entryPresentationService.Modify(id, HttpContext, cancellationToken)
            .Tap(BuildModel)
            .Finally(result => result.IsSuccess 
                ? Page() 
                : RedirectToError(result.Error));


        void BuildModel(EntryInfo entryInfo)
        {
            Id = id;
            ExpiresIn = new DateTimeOffset(entryInfo.Entry.ExpiresAt).ToUnixTimeMilliseconds();
            TextContent = entryInfo.Entry.Content;
            
            foreach (var imageUrl in entryInfo.Entry.ImageUrls)
                ImageContainers.Add(new ReadOnlyImageContainerModel(imageUrl));
        }
    }


    public async Task<IActionResult> OnPostCopy(string id, CancellationToken cancellationToken)
    {
        return await _entryPresentationService.Copy(id, HttpContext, cancellationToken)
            .Finally(result => result.IsSuccess 
                ? RedirectToPage("./Index", new { id = IdCoder.Encode(result.Value.Id) }) 
                : RedirectToError(result.Error));
    }


    public long ExpiresIn { get; set; }
    public string Id { get; set; } = default!;
    public List<ReadOnlyImageContainerModel> ImageContainers { get; set; } = [];
    public string TextContent { get; set; } = string.Empty;

    
    private readonly IEntryPresentationService _entryPresentationService;
}