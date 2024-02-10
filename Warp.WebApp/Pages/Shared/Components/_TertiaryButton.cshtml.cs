using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

namespace Warp.WebApp.Pages.Shared.Components;

public class _TertiaryButton : PageModel
{
    public IActionResult OnGet()
    {
        return Page();
    }


    public string Id { get; set; } = default!;
    public string? IconName { get; init; }
    public string MainCaption { get; init; } = default!;
    public string? SecondaryCaption { get; init; }
}