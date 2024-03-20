using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Warp.WebApp.Pages.Shared.Components;

public class OpenGraphModel : PageModel
{
    public OpenGraphModel(string title, string description, string imageUrl)
    {
        Description = description;
        ImageUrl = imageUrl;
        Title = title;
    }


    public void OnGet()
    {
    }


    public string Title { get; init; }
    public string Description { get; init; }
    public string ImageUrl { get; init; }
}