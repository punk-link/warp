using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Warp.WebApp.Pages.Shared;

public class _ModalWindowModel : PageModel
{
    public IActionResult OnGet()
    {
        return Page();
    }


    public string Action { get; set; } = "ok";
    public string Header { get; init; } = default!;
    public string Prompt { get; init; } = default!;
}