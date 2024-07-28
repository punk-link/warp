using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Entries.Enums;
using Warp.WebApp.Models.Options;
using Warp.WebApp.Pages.Shared.Components;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Entries;

namespace Warp.WebApp.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class IndexModel : BasePageModel
{
    public IndexModel(IOptionsSnapshot<AnalyticsOptions> analyticsOptions, ILoggerFactory loggerFactory, IStringLocalizer<IndexModel> localizer,
        IStringLocalizer<ServerResources> serverLocalizer, IEntryPresentationService entryPresentationService)
        : base(loggerFactory)
    {
        _analyticsOptions = analyticsOptions.Value;
        _entryPresentationService = entryPresentationService;
        _localizer = localizer;
        _serverLocalizer = serverLocalizer;
    }


    [OutputCache(Duration = 3600, VaryByQueryKeys = [nameof(id)])]
    public async Task<IActionResult> OnGet(string? id, CancellationToken cancellationToken)
    {
        AnalyticsModel = new AnalyticsModel(_analyticsOptions);

        if (string.IsNullOrEmpty(id))
        {
            OpenGraphModel = OpenGraphService.GetDefaultModel(_serverLocalizer["DefaultOpenGraphDescriptionText"]);
            ImageContainers.Add(ImageContainerModel.Empty);
            return Page();
        }

        return await _entryPresentationService.Get(id, HttpContext, cancellationToken)
            .Tap(BuildModel)
            .Tap(AddOpenGraphModel)
            .Finally(result => result.IsSuccess 
                ? Page() 
                : RedirectToError(result.Error));


        void BuildModel(EntryInfo entryInfo)
        {
            TextContent = TextFormatter.GetCleanString(entryInfo.Entry.Content);
            SelectedExpirationPeriod = GetExpirationPeriodId(entryInfo.Entry.ExpiresAt - entryInfo.Entry.CreatedAt);

            foreach (var imageId in entryInfo.ImageIds)
            {
                var imageContainer = new ImageContainerModel(imageId);
                ImageContainers.Add(imageContainer);
            }

            ImageContainers.Add(ImageContainerModel.Empty);
        }


        void AddOpenGraphModel()
        {
            OpenGraphModel = OpenGraphService.GetModel(TextContent);
        }
    }


    public async Task<IActionResult> OnPost(CancellationToken cancellationToken)
    {
        var expiresIn = GetExpirationPeriod(SelectedExpirationPeriod);
        var decodedImageIds = ImageIds.Select(IdCoder.Decode).ToList();

        var request = new EntryRequest { TextContent = TextContent, ExpiresIn = expiresIn, ImageIds = decodedImageIds, EditMode = EditMode };

        var result = await _entryPresentationService.Add(request, HttpContext, cancellationToken);
        if (result.IsFailure)
            return RedirectToError(result.Error);

        return RedirectToPage("./Preview", new { id = IdCoder.Encode(result.Value.Id) });
    }


    private static TimeSpan GetExpirationPeriod(int selectedPeriod)
        => selectedPeriod switch
        {
            1 => new TimeSpan(0, 5, 0),
            2 => new TimeSpan(0, 30, 0),
            3 => new TimeSpan(1, 0, 0),
            4 => new TimeSpan(8, 0, 0),
            5 => new TimeSpan(24, 0, 0),
            _ => new TimeSpan(0, 5, 0)
        };


    private static int GetExpirationPeriodId(TimeSpan expirationPeriod)
    {
        var ts5Min = new TimeSpan(0, 5, 0);
        var ts30Min = new TimeSpan(0, 30, 0);
        var ts1Hour = new TimeSpan(1, 0, 0);
        var ts8Hours = new TimeSpan(8, 0, 0);

        if (expirationPeriod <= ts5Min)
            return 1;
        if (expirationPeriod <= ts30Min && expirationPeriod > ts5Min)
            return 2;
        if (expirationPeriod <= ts1Hour && expirationPeriod > ts30Min)
            return 3;
        if (expirationPeriod <= ts8Hours && expirationPeriod > ts1Hour)
            return 4;

        return 5;
    }


    [BindProperty]
    public EditMode EditMode { get; set; }

    [BindProperty]
    public List<string> ImageIds { get; set; } = [];

    [BindProperty]
    public int SelectedExpirationPeriod { get; set; }

    [BindProperty]
    public string TextContent { get; set; } = string.Empty;

    public AnalyticsModel AnalyticsModel { get; set; } = default!;
    public List<ImageContainerModel> ImageContainers { get; set; } = [];
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


    private readonly AnalyticsOptions _analyticsOptions;
    private readonly IEntryPresentationService _entryPresentationService;
    private readonly IStringLocalizer<IndexModel> _localizer;
    private readonly IStringLocalizer<ServerResources> _serverLocalizer;
}