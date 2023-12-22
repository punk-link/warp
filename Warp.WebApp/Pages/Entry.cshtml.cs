
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
                ? RedirectToPage("./not-found") 
                : RedirectToError(problemDetails);
        }

        ExpiresIn = GetExpirationTimeSpan(content.CreatedAt);
        TextContent = content.Content;
        
        return Page();
    }


    private static TimeSpan GetExpirationTimeSpan(DateTime createdAt)
        => DateTime.UtcNow - createdAt;


    public string TextContent { get; set; } = string.Empty;
    public TimeSpan ExpiresIn { get; set; }

    
    private readonly IWarpContentService _warpContentService;
}