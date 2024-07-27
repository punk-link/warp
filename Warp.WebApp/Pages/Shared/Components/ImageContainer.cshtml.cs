using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Warp.WebApp.Pages.Shared.Components;

public class ImageContainerModel : PageModel
{
    public ImageContainerModel(string imageId, bool isEditable = false)
    {
        ImageId = imageId;
        IsEditable = isEditable;
    }


    public static ImageContainerModel Empty
        => new(string.Empty);


    public void OnGet()
    {
    }


    public string ImageId { get; init; }
    public bool IsEditable { get; init; }
}