using Microsoft.AspNetCore.Mvc.RazorPages;
using Warp.WebApp.Models.Images;

namespace Warp.WebApp.Pages.Shared.Components;

public class EditableImageContainerModel : PageModel
{
    public EditableImageContainerModel(Guid imageId, Uri? imageUrl)
    {
        ImageId = imageId;
        ImageUrl = imageUrl;
    }


    public EditableImageContainerModel(ImageInfo imageInfo) : this(imageInfo.Id, imageInfo.Url)
    {
    }


    private EditableImageContainerModel()
    {
    }


    public static EditableImageContainerModel Empty
        => new(Guid.Empty, null);


    public void OnGet()
    {
    }


    public Guid ImageId { get; init; }
    public Uri? ImageUrl { get; init; }
}