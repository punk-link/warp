using Microsoft.AspNetCore.Mvc.RazorPages;
using Warp.WebApp.Models.Options;

namespace Warp.WebApp.Pages.Shared.Components;

public class AnalyticsModel : PageModel
{
    public AnalyticsModel(AnalyticsOptions options)
    {
        GTag = options.GoogleGTag;
        YandexMetrikaNumber = options.YandexMetrikaNumber;
    }


    public void OnGet()
    {
    }


    public string GTag { get; init; }
    public string YandexMetrikaNumber { get; init; }
}