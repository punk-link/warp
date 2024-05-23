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
using Warp.WebApp.Pages.Shared.Components;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Entries;

namespace Warp.WebApp.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class IndexModel : BasePageModel
{
    public IndexModel(ILoggerFactory loggerFactory, IEntryService entryService) : base(loggerFactory)
    {
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

            if (this.HttpContext.User.Claims.FirstOrDefault(x => x.ValueType == ClaimTypes.UserData && x.Value == decodedId.ToString()) != null)
            {
                var (_, isFailure, entry, problemDetails) = await _entryService.Get(decodedId, cancellationToken);
                if (isFailure)
                    return RedirectToError(problemDetails);
                BuildModel(id, entry);
                return Page();
            }
        }

        OpenGraphModel = OpenGraphService.GetDefaultModel();
        return Page();


        void BuildModel(string entryId, EntryInfo entryInfo)
        {
            TextContent = entryInfo.Entry.Content;
            ImageIds = entryInfo.ImageIds;
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
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.UserData, id.ToString())
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await Response.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, new AuthenticationProperties
            {
                IsPersistent = true
            });

            return RedirectToPage("./Preview", new { id = IdCoder.Encode(id) });
        }
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

    public OpenGraphModel OpenGraphModel { get; set; } = default!;


    private readonly IEntryService _entryService;
}