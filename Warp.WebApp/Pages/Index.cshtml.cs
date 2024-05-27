using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.OutputCaching;
using System.ComponentModel;
using System.Security.Claims;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;
using Microsoft.Extensions.Options;
using Warp.WebApp.Models.Options;
using Warp.WebApp.Pages.Shared.Components;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Entries;

namespace Warp.WebApp.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class IndexModel : BasePageModel
{
    public IndexModel(IOptionsSnapshot<AnalyticsOptions> analyticsOptions, ILoggerFactory loggerFactory, IEntryService entryService) : base(loggerFactory)
    {
        _analyticsOptions = analyticsOptions.Value;
        _entryService = entryService;
    }


    [OutputCache(Duration = 3600)]
    public async Task<IActionResult> OnGet(string? id, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(id))
        {
            var decodedId = IdCoder.Decode(id);
            if (decodedId == Guid.Empty)
                return RedirectToError(ProblemDetailsHelper.Create("Can't decode a provided ID."));

            var claim = this.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData && Guid.TryParse(x.Value, out var valueGuid) ? valueGuid == decodedId : false);
            if (claim != null)
            {
                var (_, isFailure, entry, problemDetails) = await _entryService.Get(decodedId, cancellationToken);
                if (isFailure)
                    return RedirectToError(problemDetails);

                BuildModel(id, entry);
                AddOpenGraphModel();
                return Page();
            }
        }

        AnalyticsModel = new AnalyticsModel(_analyticsOptions);
        OpenGraphModel = OpenGraphService.GetDefaultModel();
        return Page();


        void BuildModel(string entryId, EntryInfo entryInfo)
        {
            TextContent = entryInfo.Entry.Content;
            ImageIds = entryInfo.ImageIds;
        }

        void AddOpenGraphModel()
        {
            OpenGraphModel = OpenGraphService.GetModel(TextContent);//, ImageUrls);
        }
    }


    public async Task<IActionResult> OnPost(CancellationToken cancellationToken)
    {
        var expiresIn = GetExpirationPeriod(SelectedExpirationPeriod);
        var (_, isFailure, id, problemDetails) = await _entryService.Add(TextContent, expiresIn, ImageIds, cancellationToken);

        if (isFailure)
            return RedirectToError(problemDetails);
        else
        {
            await ConfigureCookie(id);
            return RedirectToPage("./Preview", new { id = IdCoder.Encode(id) });
        }
    }


    private async Task ConfigureCookie(Guid entryId)
    {
        List<Claim> claims = new List<Claim>();
        if (this.HttpContext.User.Claims.Any())
            claims = this.HttpContext.User.Claims.ToList();

        claims.Add(new Claim(ClaimTypes.UserData, entryId.ToString()));
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        await Response.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, new AuthenticationProperties
        {
            IsPersistent = true
        });
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

    [DisplayName("Expires in")]
    [BindProperty]
    public int SelectedExpirationPeriod { get; set; }

    [BindProperty]
    public string TextContent { get; set; } = string.Empty;

    public static List<SelectListItem> ExpirationPeriodOptions
        =>
        [
            new SelectListItem("5 minutes", 1.ToString()),
            new SelectListItem("30 minutes", 2.ToString()),
            new SelectListItem("1 hour", 3.ToString()),
            new SelectListItem("8 hours", 4.ToString()),
            new SelectListItem("1 day", 5.ToString())
        ];


    public AnalyticsModel AnalyticsModel { get; set; } = default!;
    public OpenGraphModel OpenGraphModel { get; set; } = default!;
    
    
    private readonly AnalyticsOptions _analyticsOptions;
    private readonly IEntryService _entryService;
}