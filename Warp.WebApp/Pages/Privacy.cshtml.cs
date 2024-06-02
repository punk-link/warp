using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Warp.WebApp.Models.Options;
using Warp.WebApp.Pages.Shared.Components;

namespace Warp.WebApp.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class PrivacyModel : PageModel
{
    public PrivacyModel(IOptionsSnapshot<AnalyticsOptions> analyticsOptions)
    {
        _analyticsOptions = analyticsOptions.Value;
    }


    public IActionResult OnGet()
    {
        AnalyticsModel = new AnalyticsModel(_analyticsOptions);

        return Page();
    }


    public AnalyticsModel AnalyticsModel { get; set; } = default!;


    private readonly AnalyticsOptions _analyticsOptions;

}
