using Microsoft.AspNetCore.Mvc.RazorPages;
using Warp.WebApp.Models;

namespace Warp.WebApp.Pages.Shared.Components;

public class EditableImageContainerModel : PageModel
{
    public EditableImageContainerModel(ImageInfo imageInfo)
    {
        ImageId = imageInfo.Id;
        ImageUrl = imageInfo.Url;
    }


    private EditableImageContainerModel()
    {
    }


    public static EditableImageContainerModel Empty
        => new()
        {
            ImageId = Guid.Empty,
            ImageUrl = null
        };


    public void OnGet()
    {
    }


    public Guid ImageId { get; init; }
    public Uri? ImageUrl { get; init; }
}