using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Warp.WebApp.Pages.Shared.Components;

public class ImageContainerModel : PageModel
{
    public ImageContainerModel(Guid imageId, bool isEditable = false)
    {
        ImageId = imageId;
        IsEditable = isEditable;
    }


    public static ImageContainerModel Empty
        => new(Guid.Empty);


    public void OnGet()
    {
    }


    public Guid ImageId { get; init; }
    public bool IsEditable { get; init; }
}