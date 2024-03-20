using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Warp.WebApp.Pages.Shared.Components;

public class ModalWindowModel : PageModel
{
    public void OnGet()
    {
    }


    public string Action { get; set; } = "ok";
    public string Header { get; init; } = default!;
    public string Prompt { get; init; } = default!;
    public int TabIndex { get; init; }
    public int CancelTabIndex { get; init; }
}