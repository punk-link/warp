
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
            return RedirectToError(problemDetails);
        
        //TextContent = content.Content;
        return Page();
    }

    
    private readonly IWarpContentService _warpContentService;
}