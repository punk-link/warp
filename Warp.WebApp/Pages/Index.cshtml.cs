using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Warp.WebApp.Models.Entries;
using Warp.WebApp.Models.Entries.Enums;
using Warp.WebApp.Pages.Shared.Components;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Creators;

namespace Warp.WebApp.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class IndexModel : BasePageModel
{
    public IndexModel(
        ICookieService cookieService, 
        ICreatorService creatorService, 
        IStringLocalizer<IndexModel> localizer,
        ILoggerFactory loggerFactory)
        : base(cookieService, creatorService, loggerFactory)
    {
        _localizer = localizer;
    }


    public IActionResult OnGet(string? id)
    {
        if (!string.IsNullOrWhiteSpace(id))
            Id = id;

        ImageContainers.Add(EditableImageContainerModel.Empty);

        OpenGraphModel = new OpenGraphModel(new EntryOpenGraphDescription(_localizer["Open Graph Title"], _localizer["Open Graph Description"], null));

        return Page();
    }


    public IActionResult OnPost(CancellationToken cancellationToken)
    {
        return RedirectToPage("./Preview", new { id = Id });
    }


    [BindProperty]
    public string? Id { get; set; }

    [BindProperty]
    public EditMode EditMode { get; set; } = EditMode.Unset;

    [BindProperty]
    public List<string> ImageIds { get; set; } = [];

    [BindProperty]
    public int SelectedExpirationPeriod { get; set; }

    [BindProperty]
    public string TextContent { get; set; } = string.Empty;

    public AnalyticsModel AnalyticsModel { get; set; } = default!;
    public List<EditableImageContainerModel> ImageContainers { get; set; } = [];
    public OpenGraphModel OpenGraphModel { get; set; } = default!;

    public List<SelectListItem> ExpirationPeriodOptions
        =>
        [
            new SelectListItem(_localizer["5 minutes"], 1.ToString()),
            new SelectListItem(_localizer["30 minutes"], 2.ToString()),
            new SelectListItem(_localizer["1 hour"], 3.ToString()),
            new SelectListItem(_localizer["8 hours"], 4.ToString()),
            new SelectListItem(_localizer["1 day"], 5.ToString())
        ];


    private readonly IStringLocalizer<IndexModel> _localizer;
}
