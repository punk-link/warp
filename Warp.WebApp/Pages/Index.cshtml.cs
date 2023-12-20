using System.ComponentModel;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Warp.WebApp.Models;
using Warp.WebApp.Services;

namespace Warp.WebApp.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class IndexModel : BasePageModel
{

    public IndexModel(ILogger<IndexModel> logger, IWrapContentService wrapContentService)
    {
        _logger = logger;
        _wrapContentService = wrapContentService;
    }
    
    
    public IActionResult OnGet(Guid? id)
    {
        if (id is null)
            return Page();
        
        var (_, isFailure, content, problemDetails) = _wrapContentService.Get(id.Value);
        if (isFailure)
        {
            //_logger.LogError(error!);
            //AddProblemDetails(problemDetails);
            return RedirectToPage("./Error");
        }
        
        TextContent = content.Content;
        return Page();
    }


    public IActionResult OnPost()
    {
        var content = new WarpContent
        {
            Content = "", // TextContent,
            ExpiresIn = GetExpirationPeriod(SelectedExpirationPeriod)
        };
        
        var (_, isFailure, id, problemDetails) = _wrapContentService.Add(content);
        if (!isFailure)
            return RedirectToPage("./Index", new { id });

        return RedirectToError(problemDetails);

    }


    private static TimeSpan GetExpirationPeriod(string selectedValue)
        => selectedValue switch
        {
            "1" => new TimeSpan(0, 5, 0),
            "2" => new TimeSpan(0, 30, 0),
            "3" => new TimeSpan(1, 0, 0),
            "4" => new TimeSpan(8, 0, 0),
            "5" => new TimeSpan(24, 0, 0),
            _ => new TimeSpan(0, 5, 0)
        };

    
    [DisplayName("Expires in: ")]
    public string SelectedExpirationPeriod { get; set; }
    [BindProperty]
    public string TextContent { get; set; }
    
    public List<SelectListItem> ExpirationPeriodOptions
        =>
        [
            new SelectListItem("5 minutes", "1"),
            new SelectListItem("30 minutes", "2"),
            new SelectListItem("1 hour", "3"),
            new SelectListItem("8 hours", "4"),
            new SelectListItem("1 day", "5")
        ];
    
        
    private readonly ILogger<IndexModel> _logger;
    private readonly IWrapContentService _wrapContentService;
}