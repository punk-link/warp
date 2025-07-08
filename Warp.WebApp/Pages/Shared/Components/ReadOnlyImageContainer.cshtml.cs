using Microsoft.AspNetCore.Mvc.RazorPages;
using Warp.WebApp.Models.Images;

namespace Warp.WebApp.Pages.Shared.Components;

public class ReadOnlyImageContainerModel : PageModel
{
    public ReadOnlyImageContainerModel(ImageInfo imageInfo)
    {
        ImageUrl = imageInfo.Url;
    }


    public void OnGet()
    {
    }


    public Uri ImageUrl { get; init; }
}