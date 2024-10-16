using CSharpFunctionalExtensions;
using CSharpFunctionalExtensions.ValueTasks;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;
using Warp.WebApp.Pages.Shared.Components;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Entries;
using static System.Net.Mime.MediaTypeNames;
using Warp.WebApp.Services.Images;
using Warp.WebApp.Services.Infrastructure;

namespace Warp.WebApp.Pages;

public class PreviewModel : BasePageModel
{
    public PreviewModel(ILoggerFactory loggerFactory, IEntryPresentationService entryPresentationService, IUrlService urlService)
        : base(loggerFactory)
    {
        _entryPresentationService = entryPresentationService;
        _urlService = urlService;
    }


    public async Task<IActionResult> OnGet(string id, CancellationToken cancellationToken)
    {
        return await _entryPresentationService.Modify(id, HttpContext, cancellationToken)
            .Tap(BuildModel)
            .Finally(Redirect);


        void BuildModel(EntryInfo entryInfo)
        {
            Id = id;
            ExpiresIn = new DateTimeOffset(entryInfo.Entry.ExpiresAt).ToUnixTimeMilliseconds();
            TextContent = entryInfo.Entry.Content;
            
            foreach (var imageId in entryInfo.Entry.ImageIds)
                ImageContainers.Add(new ReadOnlyImageContainerModel(_urlService.GetImageUrl(id, imageId)));
        }


        async Task<IActionResult> Redirect(Result<EntryInfo, ProblemDetails> result)
        {
            if (result.IsSuccess)
                return Page();

            var existedResult = await _entryPresentationService.Get(id, HttpContext, cancellationToken);
            if (existedResult.IsSuccess)
                return RedirectToPage("./Entry", new { id = IdCoder.Encode(existedResult.Value.Entry.Id) });

            return RedirectToError(existedResult.Error);
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
    private readonly IUrlService _urlService;
}