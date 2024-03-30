using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Warp.WebApp.Pages.Shared.Components;

public class TabButtonModel : PageModel
{
    public void OnGet()
    {
    }


    public string Id { get; set; } = default!;
    public string Caption { get; init; } = default!;
    public bool IsDisabled { get; init; } = false;
    public int TabIndex { get; init; }
}