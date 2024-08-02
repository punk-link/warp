using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Warp.WebApp.Pages.Shared.Components;

public class EditableImageContainerModel : PageModel
{
    public EditableImageContainerModel(Guid imageId, bool isEditable = false)
    {
        ImageId = imageId;
        IsEditable = isEditable;
    }


    public static EditableImageContainerModel Empty
        => new(Guid.Empty);


    public void OnGet()
    {
    }


    public Guid ImageId { get; init; }
    public bool IsEditable { get; init; }
}