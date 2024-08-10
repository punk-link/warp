using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;
using Warp.WebApp.Pages.Shared.Components;
using Warp.WebApp.Services.Entries;

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
            .Bind(BuildModel)
            .Tap(AddOpenGraphModel)
            .Finally(result => result.IsSuccess 
                ? Page() 
                : RedirectToError(result.Error));


        Result<Entry, ProblemDetails> BuildModel(EntryInfo entryInfo)
        {
            Id = id;
            ExpiresIn = new DateTimeOffset(entryInfo.Entry.ExpiresAt).ToUnixTimeMilliseconds();
            TextContent = entryInfo.Entry.Content;
            ViewCount = entryInfo.ViewCount;

            foreach (var imageUrl in entryInfo.Entry.ImageUrls)
                ImageContainers.Add(new ReadOnlyImageContainerModel(imageUrl));

            return entryInfo.Entry;
        }


        void AddOpenGraphModel(Entry entry)
        {
            OpenGraphModel = new OpenGraphModel(entry.OpenGraphDescription);
        }
    }


    public OpenGraphModel OpenGraphModel { get; set; } = default!;


    public long ExpiresIn { get; set; }
    public string Id { get; set; } = default!;
    public List<ReadOnlyImageContainerModel> ImageContainers { get; set; } = [];
    public string TextContent { get; set; } = string.Empty;
    public long ViewCount { get; set; } = 1;


    private readonly IEntryPresentationService _entryPresentationService;
}