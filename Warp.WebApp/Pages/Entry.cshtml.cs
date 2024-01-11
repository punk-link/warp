
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Services;

namespace Warp.WebApp.Pages;

public class EntryModel : BasePageModel
{
    public EntryModel(ILoggerFactory loggerFactory, IWarpContentService warpContentService) : base(loggerFactory)
    {
        _warpContentService = warpContentService;
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

        ExpiresIn = GetExpirationMessage(content.ExpiresAt);
        TextContent = TextFormatter.Format(content.Content);
        
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


    public string TextContent { get; set; } = string.Empty;
    public string ExpiresIn { get; set; } = string.Empty; 

    
    private readonly IWarpContentService _warpContentService;
}