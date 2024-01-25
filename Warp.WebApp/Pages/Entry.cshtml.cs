using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Pages.Shared;
using Warp.WebApp.Services;
using Warp.WebApp.Utils;

namespace Warp.WebApp.Pages;

public class EntryModel : BasePageModel
{
    public EntryModel(ILoggerFactory loggerFactory, IWarpContentService warpContentService, IViewCountService viewCountService) : base(loggerFactory)
    {
        _warpContentService = warpContentService;
        _viewCountService = viewCountService;
    }
    
    
    public IActionResult OnGet(Guid id)
    {
        var (_, isFailure, content, problemDetails) = _warpContentService.Get(id);
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

        ModalWindowModel = new _ModalWindowModel
        {
            Action = "report",
            Header = "report entry",
            Prompt = "You are about to report this content. This action restricts an access to the content for all viewers. Are you sure?"
        };
        
        return Page();
    }


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


    public string ExpiresIn { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public _ModalWindowModel ModalWindowModel { get; set; } = default!;
    public string TextContent { get; set; } = string.Empty;
    public int ViewCount { get; set; } = 1;

    
    private readonly IWarpContentService _warpContentService;
    private readonly IViewCountService _viewCountService;
}