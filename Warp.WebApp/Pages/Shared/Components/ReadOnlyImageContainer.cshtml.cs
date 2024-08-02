using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Warp.WebApp.Pages.Shared.Components;

public class ReadOnlyImageContainerModel : PageModel
{
    public ReadOnlyImageContainerModel(Uri imageUrl)
    {
        ImageUrl = imageUrl;
    }


    public void OnGet()
    {
    }


    public Uri ImageUrl { get; init; }
}