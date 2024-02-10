using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Pages.Shared;
using Warp.WebApp.Pages.Shared.Components;
using Warp.WebApp.Services;

namespace Warp.WebApp.Pages;

public class EntryModel : BasePageModel
{
    public EntryModel(ILoggerFactory loggerFactory, IWarpContentService warpContentService, IViewCountService viewCountService, IImageService imageService) 
        : base(loggerFactory)
    {
        _imageService = imageService;
        _viewCountService = viewCountService;
        _warpContentService = warpContentService;
    }
    
    
    public async Task<IActionResult> OnGet(Guid id)
    {
        var (_, isFailure, content, problemDetails) = await _warpContentService.Get(id);
        if (isFailure)
        {
            return problemDetails.Status == StatusCodes.Status404NotFound 
                ? RedirectToPage("./NotFound") 
                : RedirectToError(problemDetails);
        }

        Id = id;
        ExpiresIn = GetExpirationMessage(content.ExpiresAt);
        TextContent = TextFormatter.Format(content.Content);
        ViewCount = _viewCountService.AddAndGet(id);

        var imageIds = (await _imageService.Get(id))
            .Select(image => image.Id)
            .ToList();
        ImageUrls = BuildImageUrls(id, imageIds);

        return AddButtonModels();


        IActionResult AddButtonModels()
        {
            CopyButtonModel = new _TertiaryButton
            {
                Id = "copy-url-button",
                IconName = "icofont-copy",
                MainCaption = "copy link",
                SecondaryCaption = "copied"
            };
            ModalWindowModel = new _ModalWindowModel
            {
                Action = "report",
                Header = "report entry",
                Prompt = "You are about to report this content. This action restricts an access to the content for all viewers. Are you sure?"
            };
            ReportButtonModel = new _TertiaryButton
            {
                Id = "report-button",
                MainCaption = "report"
            };
        
            return Page();
        }
    }


    private static List<string> BuildImageUrls(Guid id, List<Guid> imageIds)
        => imageIds.Select(imageId => $"/api/images/entry-id/{id}/image-id/{imageId}")
            .ToList();


    private static string GetExpirationMessage(DateTime expiresAt)
    {
        var timeSpan = GetExpirationTimeSpan(expiresAt);
        if (1 < timeSpan.Days)
            return $"The entry expires in {timeSpan.Days} days";

        if (1 < timeSpan.Hours)
            return $"The entry expires in {timeSpan.Hours} hours";

        if (1 < timeSpan.Minutes)
            return $"The entry expires in {timeSpan.Minutes} minutes";

        if (1 < timeSpan.Seconds)
            return $"The entry expires in {timeSpan.Seconds} seconds";
        
        return "The entry expires now";
    }


    private static TimeSpan GetExpirationTimeSpan(DateTime expiresAt)
        => expiresAt - DateTime.UtcNow;

    
    public _TertiaryButton CopyButtonModel { get; set; } = default!;
    public _ModalWindowModel ModalWindowModel { get; set; } = default!;
    public _TertiaryButton ReportButtonModel { get; set; } = default!;


    public string ExpiresIn { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public List<string> ImageUrls { get; set; } = [];
    public string TextContent { get; set; } = string.Empty;
    public int ViewCount { get; set; } = 1;
    
    
    private readonly IImageService _imageService;
    private readonly IViewCountService _viewCountService;
    private readonly IWarpContentService _warpContentService;
}