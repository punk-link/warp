using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Warp.WebApp.Pages.Shared.Components;

public class SecondaryButtonModel : PageModel
{
    public void OnGet()
    {
    }


    public string Id { get; set; } = default!;
    public string? IconName { get; init; }
    public string MainCaption { get; init; } = default!;
    public string? SecondaryCaption { get; init; }
    public int TabIndex { get; init; }
}