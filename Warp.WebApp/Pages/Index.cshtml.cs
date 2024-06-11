using System.ComponentModel;
using Consul;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using System.Security.Claims;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Options;
using Microsoft.Extensions.Localization;
using Warp.WebApp.Pages.Shared.Components;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Entries;
using Warp.WebApp.Services.User;

namespace Warp.WebApp.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class IndexModel : BasePageModel
{
    public IndexModel(IOptionsSnapshot<AnalyticsOptions> analyticsOptions, ILoggerFactory loggerFactory, IStringLocalizer<IndexModel> localizer, IEntryService entryService, ICookieService cookieService) 
        : base(loggerFactory)
    {
        _analyticsOptions = analyticsOptions.Value;
        _entryService = entryService;
        _localizer = localizer;
        _cookieService = cookieService;
    }


    [OutputCache(Duration = 3600)]
    public async Task<IActionResult> OnGet(string? id, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(id))
        {
            var decodedId = IdCoder.Decode(id);
            if (decodedId == Guid.Empty)
                return RedirectToError(ProblemDetailsHelper.Create("Can't decode a provided ID."));

            var claim = CookieService.GetClaim(HttpContext);
            if (claim != null)
            {
                var userId = Guid.Parse(claim.Value);
                var (_, isFailure, entry, problemDetails) = await _entryService.Get(userId, decodedId, cancellationToken);
                if (isFailure)
                    return RedirectToError(problemDetails);

                BuildModel(entry);
                AddOpenGraphModel();
                return Page();
            }
        }

        AnalyticsModel = new AnalyticsModel(_analyticsOptions);
        OpenGraphModel = OpenGraphService.GetDefaultModel();
        return Page();


        void BuildModel(EntryInfo entryInfo)
        {
            TextContent = TextFormatter.GetCleanString(entryInfo.Entry.Content);
            ImageIds = entryInfo.ImageIds;
            SelectedExpirationPeriod = GetExpirationPeriodId(entryInfo.Entry.ExpiresAt - entryInfo.Entry.CreatedAt);
        }

        void AddOpenGraphModel()
        {
            OpenGraphModel = OpenGraphService.GetModel(TextContent);//, ImageUrls);
        }
    }


    public async Task<IActionResult> OnPost(CancellationToken cancellationToken)
    {
        var expiresIn = GetExpirationPeriod(SelectedExpirationPeriod);
        var userId = await _cookieService.ConfigureCookie(HttpContext, Response);
        var (_, isFailure, id, problemDetails) = await _entryService.Add(userId, TextContent, expiresIn, ImageIds, cancellationToken);

        if (isFailure)
            return RedirectToError(problemDetails);

        return RedirectToPage("./Preview", new { id = IdCoder.Encode(id) });
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


    private static int GetExpirationPeriodId(TimeSpan expirationPeriod)
    {
        var ts5min = new TimeSpan(0, 5, 0);
        var ts30min = new TimeSpan(0, 30, 0);
        var ts1hour = new TimeSpan(1, 0, 0);
        var ts8hours = new TimeSpan(8, 0, 0);

        if (expirationPeriod <= ts5min)
            return 1;
        if (expirationPeriod <= ts30min && expirationPeriod > ts5min)
            return 2;
        if (expirationPeriod <= ts1hour && expirationPeriod > ts30min)
            return 3;
        if (expirationPeriod <= ts8hours && expirationPeriod > ts1hour)
            return 4;

        return 5;
    }


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
    private readonly ICookieService _cookieService;
}