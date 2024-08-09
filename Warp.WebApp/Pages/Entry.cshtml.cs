using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;
using Warp.WebApp.Pages.Shared.Components;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Entries;
using Warp.WebApp.Services.Infrastructure;

namespace Warp.WebApp.Pages;

public class EntryModel : BasePageModel
{
    public EntryModel(ILoggerFactory loggerFactory, IEntryPresentationService entryPresentationService, IUrlService urlService)
        : base(loggerFactory)
    {
        _entryPresentationService = entryPresentationService;
        _urlService = urlService;
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

            foreach (var imageId in entryInfo.ImageIds)
            {
                var url = _urlService.GetImageUrl(id, in imageId);
                ImageContainers.Add(new ReadOnlyImageContainerModel(url));
            }
        }

        // TODO: move closer to the initialization of the model
        void AddOpenGraphModel()
        {
            if (ImageContainers.Count == 0)
                return;

            var imageUrls = ImageContainers.Select(x => x.ImageUrl).ToList();
            OpenGraphModel = OpenGraphService.GetModel(TextContent, imageUrls);
        }
    }


    public OpenGraphModel OpenGraphModel { get; set; } = default!;


    public long ExpiresIn { get; set; }
    public string Id { get; set; } = default!;
    public List<ReadOnlyImageContainerModel> ImageContainers { get; set; } = [];
    public string TextContent { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public long ViewCount { get; set; } = 1;


    private readonly IEntryPresentationService _entryPresentationService;
    private readonly IUrlService _urlService;
}