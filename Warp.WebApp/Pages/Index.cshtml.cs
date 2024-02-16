using System.ComponentModel;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Warp.WebApp.Services;

namespace Warp.WebApp.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class IndexModel : BasePageModel
{
    public IndexModel(ILoggerFactory loggerFactory, IEntryService entryService, IImageService imageService) : base(loggerFactory)
    {
        _imageService = imageService;
        _entryService = entryService;
    }
    
    
    public IActionResult OnGet()
        => Page();


    public async Task<IActionResult> OnPost()
    {
        var expiresIn = GetExpirationPeriod(SelectedExpirationPeriod);
        var (_, isFailure, id, problemDetails) = await _entryService.Add(TextContent, expiresIn, ImageIds);
        if (isFailure)
            return RedirectToError(problemDetails);

        await _imageService.Attach(id, expiresIn, ImageIds);

        return RedirectToPage("./Entry", new { id });
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


    [BindProperty]
    public List<Guid> ImageIds { get; set; } = [];

    [DisplayName("Expires in")]
    [BindProperty]
    public string SelectedExpirationPeriod { get; set; } = string.Empty;

    [BindProperty]
    public string TextContent { get; set; } = string.Empty;
    
    public List<SelectListItem> ExpirationPeriodOptions
        =>
        [
            new SelectListItem("5 minutes", "1"),
            new SelectListItem("30 minutes", "2"),
            new SelectListItem("1 hour", "3"),
            new SelectListItem("8 hours", "4"),
            new SelectListItem("1 day", "5")
        ];
    
        
    private readonly IEntryService _entryService;
    private readonly IImageService _imageService;
}