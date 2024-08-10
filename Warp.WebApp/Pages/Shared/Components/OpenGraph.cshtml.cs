using Microsoft.AspNetCore.Mvc.RazorPages;
using Warp.WebApp.Models.Entries;

namespace Warp.WebApp.Pages.Shared.Components;

public class OpenGraphModel : PageModel
{
    public OpenGraphModel(EntryOpenGraphDescription openGraphDescription)
    {
        Description = openGraphDescription.Description;
        ImageUrl = openGraphDescription.ImageUrl;
        Title = openGraphDescription.Title;
    }


    public void OnGet()
    {
    }


    public string Title { get; init; }
    public string Description { get; init; }
    public Uri? ImageUrl { get; init; }
}