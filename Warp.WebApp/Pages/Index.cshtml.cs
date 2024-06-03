using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Options;
using Warp.WebApp.Models.Options;
using Microsoft.Extensions.Localization;
using Warp.WebApp.Pages.Shared.Components;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Entries;

namespace Warp.WebApp.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class IndexModel : BasePageModel
{
    public IndexModel(IOptionsSnapshot<AnalyticsOptions> analyticsOptions, ILoggerFactory loggerFactory, IStringLocalizer<IndexModel> localizer, IEntryService entryService) 
        : base(loggerFactory)
    {
        _analyticsOptions = analyticsOptions.Value;
        _entryService = entryService;
        _localizer = localizer;
    }


    [OutputCache(Duration = 3600)]
    public IActionResult OnGet()
    {
        AnalyticsModel = new AnalyticsModel(_analyticsOptions);
        OpenGraphModel = OpenGraphService.GetDefaultModel();

        return Page();
    }


    public async Task<IActionResult> OnPost(CancellationToken cancellationToken)
    {
        var expiresIn = GetExpirationPeriod(SelectedExpirationPeriod);
        var (_, isFailure, id, problemDetails) = await _entryService.Add(TextContent, expiresIn, ImageIds, cancellationToken);
        
        return isFailure 
            ? RedirectToError(problemDetails) 
            : RedirectToPage("./Entry", new { id = IdCoder.Encode(id) });
    }


    private static TimeSpan GetExpirationPeriod(int selectedPeriod)
        => selectedPeriod switch
        {
            1 => new TimeSpan(0, 5, 0),
            2 => new TimeSpan(0, 30, 0),
            3 => new TimeSpan(1, 0, 0),
            4 => new TimeSpan(8, 0, 0),
            5 => new TimeSpan(24, 0, 0),
            _ => new TimeSpan(0, 5, 0)
        };


    [BindProperty]
    public List<Guid> ImageIds { get; set; } = [];

    [BindProperty]
    public int SelectedExpirationPeriod { get; set; }

    [BindProperty]
    public string TextContent { get; set; } = string.Empty;
    
    public List<SelectListItem> ExpirationPeriodOptions
        =>
        [
            new SelectListItem(_localizer["5 minutes"], 1.ToString()),
            new SelectListItem(_localizer["30 minutes"], 2.ToString()),
            new SelectListItem(_localizer["1 hour"], 3.ToString()),
            new SelectListItem(_localizer["8 hours"], 4.ToString()),
            new SelectListItem(_localizer["1 day"], 5.ToString())
        ];


    public AnalyticsModel AnalyticsModel { get; set; } = default!;
    public OpenGraphModel OpenGraphModel { get; set; } = default!;
    
    
    private readonly AnalyticsOptions _analyticsOptions;
    private readonly IEntryService _entryService;
    private readonly IStringLocalizer<IndexModel> _localizer;
}